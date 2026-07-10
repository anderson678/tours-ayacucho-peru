using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToursAyacuchoPeruAPI.Application.DTOs.Reservations;
using ToursAyacuchoPeruAPI.Application.Interfaces;

namespace ToursAyacuchoPeruAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/admin/reservations")]
    [Authorize(Roles = "Administrador")]
    public class AdminReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public AdminReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        /// <summary>SD-Admin: Historial global de reservas con datos de cliente, paquete y pago.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AdminReservationResponseDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> GetAll([FromQuery] string? estado, [FromQuery] Guid? paqueteId)
        {
            var reservations = await _reservationService.GetAllForAdminAsync(estado, paqueteId);
            return Ok(reservations);
        }
    }
}
