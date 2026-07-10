using System.Threading.Tasks;
using ToursAyacuchoPeruAPI.Application.DTOs.SiteSettings;

namespace ToursAyacuchoPeruAPI.Application.Interfaces
{
    public interface ISiteSettingsService
    {
        Task<SiteSettingsDto> GetAsync();
        Task<SiteSettingsDto> UpdateAsync(SiteSettingsDto dto);
    }
}
