using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ToursAyacuchoPeruAPI.Application.DTOs.Reports;
using ToursAyacuchoPeruAPI.Application.Interfaces;
using ToursAyacuchoPeruAPI.Domain.Enums;
using ToursAyacuchoPeruAPI.Domain.Exceptions;
using ToursAyacuchoPeruAPI.Infrastructure.Persistence;

namespace ToursAyacuchoPeruAPI.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly ToursAyacuchoPeruDbContext _db;

        public ReportService(ToursAyacuchoPeruDbContext db)
        {
            _db = db;
        }

        public async Task<ReportFileDto> GenerateReservationsReportAsync(ReportRequestDto request)
        {
            ValidateRequest(request);
            var estadoFilter = ParseEstadoFilter(request.Estado);

            var query = _db.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Paquete)
                .Where(r => r.FechaCreacion.Date >= request.From.Date && r.FechaCreacion.Date <= request.To.Date)
                .AsNoTracking();

            if (estadoFilter.HasValue)
                query = query.Where(r => r.Estado == estadoFilter.Value);

            if (request.PaqueteId.HasValue)
                query = query.Where(r => r.PaqueteId == request.PaqueteId.Value);

            var rows = await query
                .OrderBy(r => r.FechaCreacion)
                .Select(r => new[]
                {
                    r.ReservaId.ToString(),
                    r.Usuario.Nombre,
                    r.Paquete.Nombre,
                    r.FechaInicio.ToString("yyyy-MM-dd"),
                    r.CantAsientos.ToString(CultureInfo.InvariantCulture),
                    r.Estado.ToString(),
                    r.MontoTotal.ToString("F2", CultureInfo.InvariantCulture)
                })
                .ToListAsync();

            var headers = new[] { "ReservaId", "Cliente", "Paquete", "Fecha", "Asientos", "Estado", "Monto" };
            return BuildReport("reporte-reservas", "Reporte de reservas", headers, rows, request);
        }

        public async Task<ReportFileDto> GenerateSalesReportAsync(ReportRequestDto request)
        {
            ValidateRequest(request);
            var estadoFilter = ParseEstadoFilter(request.Estado) ?? EstadoReserva.CONFIRMADA;

            var query = _db.Pagos
                .Include(p => p.Reserva).ThenInclude(r => r.Paquete)
                .Where(p => p.FechaPago.Date >= request.From.Date && p.FechaPago.Date <= request.To.Date
                    && p.Reserva.Estado == estadoFilter)
                .AsNoTracking();

            if (request.PaqueteId.HasValue)
                query = query.Where(p => p.Reserva.PaqueteId == request.PaqueteId.Value);

            var payments = await query
                .ToListAsync();

            var rows = payments
                .GroupBy(p => p.Reserva.Paquete.Nombre)
                .OrderBy(g => g.Key)
                .Select(g => new[]
                {
                    g.Key,
                    g.Count().ToString(CultureInfo.InvariantCulture),
                    g.Sum(p => p.Monto).ToString("F2", CultureInfo.InvariantCulture)
                })
                .ToList();

            rows.Insert(0, new[]
            {
                "TOTAL",
                payments.Count.ToString(CultureInfo.InvariantCulture),
                payments.Sum(p => p.Monto).ToString("F2", CultureInfo.InvariantCulture)
            });

            var headers = new[] { "Paquete", "Reservas", "Ingresos" };
            return BuildReport("reporte-ventas", "Reporte de ventas", headers, rows, request);
        }

        private static void ValidateRequest(ReportRequestDto request)
        {
            if (request.From == default || request.To == default)
                throw new UnprocessableEntityException("El rango de fechas del reporte es requerido.", "RANGO_FECHAS_INVALIDO");

            if (request.From.Date > request.To.Date)
                throw new UnprocessableEntityException("La fecha de inicio debe ser anterior o igual a la fecha de fin.", "RANGO_FECHAS_INVALIDO");

            var months = ((request.To.Year - request.From.Year) * 12) + request.To.Month - request.From.Month;
            if (months > 12 || (months == 12 && request.To.Day > request.From.Day))
                throw new UnprocessableEntityException("El rango del reporte no puede exceder 12 meses.", "RANGO_FECHAS_INVALIDO");

            if (!IsPdf(request.Format) && !IsXlsx(request.Format))
                throw new UnprocessableEntityException("Formato de reporte no válido. Use pdf o xlsx.", "FORMATO_REPORTE_INVALIDO");
        }

        private static EstadoReserva? ParseEstadoFilter(string? estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
                return null;

            if (Enum.TryParse<EstadoReserva>(estado.Trim(), ignoreCase: true, out var parsed)
                && Enum.IsDefined(typeof(EstadoReserva), parsed))
            {
                return parsed;
            }

            throw new UnprocessableEntityException("Estado de reserva no valido.", "ESTADO_REPORTE_INVALIDO");
        }

        private static ReportFileDto BuildReport(
            string baseFileName,
            string title,
            IReadOnlyList<string> headers,
            IReadOnlyList<string[]> rows,
            ReportRequestDto request)
        {
            if (IsXlsx(request.Format))
            {
                return new ReportFileDto
                {
                    Content = BuildXlsx(title, headers, rows, request),
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    FileName = $"{baseFileName}.xlsx"
                };
            }

            return new ReportFileDto
            {
                Content = BuildPdf(title, headers, rows, request),
                ContentType = "application/pdf",
                FileName = $"{baseFileName}.pdf"
            };
        }

        private static bool IsPdf(string format) => string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase);
        private static bool IsXlsx(string format) => string.Equals(format, "xlsx", StringComparison.OrdinalIgnoreCase);

        private static byte[] BuildPdf(string title, IReadOnlyList<string> headers, IReadOnlyList<string[]> rows, ReportRequestDto request)
        {
            var lines = new List<string>
            {
                title,
                $"Periodo: {request.From:yyyy-MM-dd} a {request.To:yyyy-MM-dd}",
                DescribeFilters(request),
                string.Join(" | ", headers)
            };
            lines.AddRange(rows.Select(row => string.Join(" | ", row)));

            var content = new StringBuilder();
            content.Append("BT\n/F1 10 Tf\n50 780 Td\n");
            foreach (var line in lines.Take(55))
            {
                var pdfLine = ToPdfSafeText(line.Length > 110 ? line[..110] : line);
                content.Append('(').Append(EscapePdf(pdfLine)).Append(") Tj\n0 -14 Td\n");
            }
            content.Append("ET");

            var stream = content.ToString();
            var objects = new List<string>
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
                $"<< /Length {Encoding.ASCII.GetByteCount(stream)} >>\nstream\n{stream}\nendstream"
            };

            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, Encoding.ASCII, leaveOpen: true);
            writer.Write("%PDF-1.4\n");
            var offsets = new List<long> { 0 };
            for (var i = 0; i < objects.Count; i++)
            {
                offsets.Add(ms.Position);
                writer.Write($"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
                writer.Flush();
            }

            var xrefOffset = ms.Position;
            writer.Write($"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");
            foreach (var offset in offsets.Skip(1))
                writer.Write($"{offset:0000000000} 00000 n \n");
            writer.Write($"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF");
            writer.Flush();
            return ms.ToArray();
        }

        private static string EscapePdf(string value)
        {
            return value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        }

        private static string ToPdfSafeText(string value)
        {
            return string.Concat(value.Select(c => c <= 127 ? c : '?'));
        }

        private static string DescribeFilters(ReportRequestDto request)
        {
            var estado = string.IsNullOrWhiteSpace(request.Estado) ? "Todos" : request.Estado.Trim();
            var paquete = request.PaqueteId.HasValue ? request.PaqueteId.Value.ToString() : "Todos";
            return $"Filtros: Estado={estado}; Paquete={paquete}";
        }

        private static byte[] BuildXlsx(string title, IReadOnlyList<string> headers, IReadOnlyList<string[]> rows, ReportRequestDto request)
        {
            using var ms = new MemoryStream();
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
            {
                AddEntry(archive, "[Content_Types].xml", "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/><Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/><Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/></Types>");
                AddEntry(archive, "_rels/.rels", "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>");
                AddEntry(archive, "xl/_rels/workbook.xml.rels", "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/></Relationships>");
                AddEntry(archive, "xl/workbook.xml", "<?xml version=\"1.0\" encoding=\"UTF-8\"?><workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets><sheet name=\"Reporte\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>");
                AddEntry(archive, "xl/worksheets/sheet1.xml", BuildWorksheetXml(title, headers, rows, request));
            }
            return ms.ToArray();
        }

        private static string BuildWorksheetXml(string title, IReadOnlyList<string> headers, IReadOnlyList<string[]> rows, ReportRequestDto request)
        {
            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>");
            AppendRow(sb, new[] { title });
            AppendRow(sb, new[] { $"Periodo: {request.From:yyyy-MM-dd} a {request.To:yyyy-MM-dd}" });
            AppendRow(sb, new[] { DescribeFilters(request) });
            AppendRow(sb, headers);
            foreach (var row in rows)
                AppendRow(sb, row);
            sb.Append("</sheetData></worksheet>");
            return sb.ToString();
        }

        private static void AppendRow(StringBuilder sb, IReadOnlyList<string> values)
        {
            sb.Append("<row>");
            foreach (var value in values)
                sb.Append("<c t=\"inlineStr\"><is><t>").Append(EscapeXml(value)).Append("</t></is></c>");
            sb.Append("</row>");
        }

        private static string EscapeXml(string value)
        {
            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;");
        }

        private static void AddEntry(ZipArchive archive, string path, string content)
        {
            var entry = archive.CreateEntry(path);
            using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
            writer.Write(content);
        }
    }
}
