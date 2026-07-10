using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToursAyacuchoPeruAPI.Application.DTOs.Reports;
using ToursAyacuchoPeruAPI.Application.Interfaces;

namespace ToursAyacuchoPeruAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/admin/reports")]
    [Authorize(Roles = "Administrador")]
    public class AdminReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public AdminReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("reservations")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> Reservations(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] string format = "pdf",
            [FromQuery] string? estado = null,
            [FromQuery] Guid? paqueteId = null)
        {
            var report = await _reportService.GenerateReservationsReportAsync(new ReportRequestDto
            {
                From = from,
                To = to,
                Format = format,
                Estado = estado,
                PaqueteId = paqueteId
            });

            return File(report.Content, report.ContentType, report.FileName);
        }

        [HttpGet("sales")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> Sales(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] string format = "pdf",
            [FromQuery] string? estado = null,
            [FromQuery] Guid? paqueteId = null)
        {
            var report = await _reportService.GenerateSalesReportAsync(new ReportRequestDto
            {
                From = from,
                To = to,
                Format = format,
                Estado = estado,
                PaqueteId = paqueteId
            });

            return File(report.Content, report.ContentType, report.FileName);
        }
    }
}
