// Tarea 5.x, 7.x â€” SD-04, SD-06, SD-07: ReservationController â€” TOURS AYACUCHO PERÃš
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToursAyacuchoPeruAPI.Presentation.Extensions;
using ToursAyacuchoPeruAPI.Application.Interfaces;
using ToursAyacuchoPeruAPI.Application.DTOs.Reservations;

namespace ToursAyacuchoPeruAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/reservations")]
    [Authorize]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        private readonly IReschedulingService _reschedulingService;

        public ReservationController(IReservationService reservationService, IReschedulingService reschedulingService)
        {
            _reservationService = reservationService;
            _reschedulingService = reschedulingService;
        }

        /// <summary>SD-04: Crear reserva (RF04, RF05). POST /api/v1/reservations</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ReservationResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(409)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
        {
            var clientId = User.GetClientId();
            var result = await _reservationService.CreateAsync(clientId, dto);
            return StatusCode(201, result);
        }

        /// <summary>SD-07: Listar reservas del Cliente autenticado (RF08). GET /api/v1/reservations</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ReservationResponseDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetByClient([FromQuery] string? estado)
        {
            var clientId = User.GetClientId();
            var result = await _reservationService.GetByClientAsync(clientId, estado);
            return Ok(result);
        }

        /// <summary>SD-07: Consultar detalle de una Reserva (RF08). GET /api/v1/reservations/{id}</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ReservationResponseDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var clientId = User.GetClientId();
            var result = await _reservationService.GetByIdAsync(clientId, id);
            return Ok(result);
        }

        /// <summary>SD-06: Reprogramar una Reserva confirmada (CU NÂ°05). PATCH /api/v1/reservations/{id}/reschedule</summary>
        [HttpPatch("{id}/reschedule")]
        [ProducesResponseType(typeof(ReservationResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> Reschedule([FromRoute] Guid id, [FromBody] RescheduleRequestDto dto)
        {
            var clientId = User.GetClientId();
            var result = await _reschedulingService.RescheduleAsync(clientId, id, dto);
            return Ok(result);
        }
    }
}

