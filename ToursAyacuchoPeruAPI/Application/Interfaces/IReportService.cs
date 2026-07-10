using System.Threading.Tasks;
using ToursAyacuchoPeruAPI.Application.DTOs.Reports;

namespace ToursAyacuchoPeruAPI.Application.Interfaces
{
    public interface IReportService
    {
        Task<ReportFileDto> GenerateReservationsReportAsync(ReportRequestDto request);
        Task<ReportFileDto> GenerateSalesReportAsync(ReportRequestDto request);
    }
}
