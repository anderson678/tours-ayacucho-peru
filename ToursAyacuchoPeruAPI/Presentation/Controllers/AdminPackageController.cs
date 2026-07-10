using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToursAyacuchoPeruAPI.Application.DTOs.Packages;
using ToursAyacuchoPeruAPI.Application.Interfaces;

namespace ToursAyacuchoPeruAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/admin/packages")]
    [Authorize(Roles = "Administrador")]
    public class AdminPackageController : ControllerBase
    {
        private readonly IPackageService _packageService;

        public AdminPackageController(IPackageService packageService)
        {
            _packageService = packageService;
        }

        /// <summary>SD-10: Crear paquete turístico.</summary>
        /// <summary>SD-10: Listar todos los paquetes turisticos para administracion.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PackageResponseDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetAll()
        {
            var packages = await _packageService.GetAllPackagesAsync();
            return Ok(packages);
        }

        [HttpPost]
        [ProducesResponseType(typeof(PackageResponseDto), 201)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> Create([FromBody] CreatePackageDto dto)
        {
            var package = await _packageService.CreateAsync(dto);
            return StatusCode(201, package);
        }

        /// <summary>SD-10: Actualizar paquete turístico.</summary>
        [HttpPut("{packageId:guid}")]
        [ProducesResponseType(typeof(PackageResponseDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> Update(Guid packageId, [FromBody] UpdatePackageDto dto)
        {
            var package = await _packageService.UpdateAsync(packageId, dto);
            return Ok(package);
        }

        /// <summary>SD-10: Desactivar paquete turístico mediante eliminación lógica.</summary>
        [HttpDelete("{packageId:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Deactivate(Guid packageId)
        {
            await _packageService.DeactivateAsync(packageId);
            return NoContent();
        }
    }
}
