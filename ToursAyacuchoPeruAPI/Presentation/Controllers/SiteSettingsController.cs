using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToursAyacuchoPeruAPI.Application.DTOs.SiteSettings;
using ToursAyacuchoPeruAPI.Application.Interfaces;

namespace ToursAyacuchoPeruAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/site-settings")]
    public class SiteSettingsController : ControllerBase
    {
        private readonly ISiteSettingsService _siteSettingsService;

        public SiteSettingsController(ISiteSettingsService siteSettingsService)
        {
            _siteSettingsService = siteSettingsService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(SiteSettingsDto), 200)]
        public async Task<IActionResult> Get()
        {
            var settings = await _siteSettingsService.GetAsync();
            return Ok(settings);
        }
    }

    [ApiController]
    [Route("api/v1/admin/site-settings")]
    [Authorize(Roles = "Administrador")]
    public class AdminSiteSettingsController : ControllerBase
    {
        private readonly ISiteSettingsService _siteSettingsService;

        public AdminSiteSettingsController(ISiteSettingsService siteSettingsService)
        {
            _siteSettingsService = siteSettingsService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(SiteSettingsDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> Get()
        {
            var settings = await _siteSettingsService.GetAsync();
            return Ok(settings);
        }

        [HttpPut]
        [ProducesResponseType(typeof(SiteSettingsDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> Update([FromBody] SiteSettingsDto dto)
        {
            var settings = await _siteSettingsService.UpdateAsync(dto);
            return Ok(settings);
        }
    }
}
