using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToursAyacuchoPeruAPI.Application.DTOs.Admin;
using ToursAyacuchoPeruAPI.Application.Interfaces;

namespace ToursAyacuchoPeruAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/admin/clients")]
    [Authorize(Roles = "Administrador")]
    public class AdminClientController : ControllerBase
    {
        private readonly IAdminClientService _adminClientService;

        public AdminClientController(IAdminClientService adminClientService)
        {
            _adminClientService = adminClientService;
        }

        /// <summary>SD-08: Listar cuentas de Clientes registrados.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AdminClientResponseDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetClients()
        {
            var clients = await _adminClientService.GetClientsAsync();
            return Ok(clients);
        }

        /// <summary>SD-08: Activar o desactivar una cuenta de Cliente.</summary>
        [HttpPatch("{clientId:guid}/status")]
        [ProducesResponseType(typeof(AdminClientResponseDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> UpdateStatus(Guid clientId, [FromBody] UpdateClientStatusDto dto)
        {
            var client = await _adminClientService.UpdateClientStatusAsync(clientId, dto);
            return Ok(client);
        }
    }
}
