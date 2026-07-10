// Tarea 4.x - SD-10: PackageController - TOURS AYACUCHO PERU
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToursAyacuchoPeruAPI.Application.DTOs.Packages;
using ToursAyacuchoPeruAPI.Application.DTOs.Reviews;
using ToursAyacuchoPeruAPI.Application.Interfaces;
using ToursAyacuchoPeruAPI.Presentation.Extensions;

namespace ToursAyacuchoPeruAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/packages")]
    public class PackageController : ControllerBase
    {
        private readonly IPackageService _packageService;
        private readonly IReviewService _reviewService;

        public PackageController(IPackageService packageService, IReviewService reviewService)
        {
            _packageService = packageService;
            _reviewService = reviewService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PackageResponseDto>), 200)]
        public async Task<IActionResult> GetActivePackages()
        {
            var list = await _packageService.GetActivePackagesAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PackageResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var paquete = await _packageService.GetByIdAsync(id);
            return Ok(paquete);
        }

        /// <summary>SD-09: Publicar comentario y calificación de un tour completado.</summary>
        [HttpPost("{id:guid}/reviews")]
        [Authorize]
        [ProducesResponseType(typeof(ReviewResponseDto), 201)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> CreateReview([FromRoute] Guid id, [FromBody] CreateReviewDto dto)
        {
            var clientId = User.GetClientId();
            var review = await _reviewService.CreateAsync(clientId, id, dto);
            return StatusCode(201, review);
        }
    }
}
