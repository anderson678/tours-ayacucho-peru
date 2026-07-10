using System;

namespace ToursAyacuchoPeruAPI.Application.DTOs.Reports
{
    public class ReportRequestDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string Format { get; set; } = "pdf";
        public string? Estado { get; set; }
        public Guid? PaqueteId { get; set; }
    }

    public class ReportFileDto
    {
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = null!;
        public string FileName { get; set; } = null!;
    }
}
