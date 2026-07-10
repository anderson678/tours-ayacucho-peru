// Tarea 6.x â€” SD-05: PaymentController â€” TOURS AYACUCHO PERÃš
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToursAyacuchoPeruAPI.Presentation.Extensions;
using ToursAyacuchoPeruAPI.Application.Interfaces;
using ToursAyacuchoPeruAPI.Application.DTOs.Reservations;

namespace ToursAyacuchoPeruAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/payments")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>SD-05: Registrar pago de una Reserva (RF06, RF07). POST /api/v1/payments</summary>
        [HttpPost]
        [ProducesResponseType(typeof(PaymentResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> Register([FromBody] RegisterPaymentDto dto)
        {
            var clientId = User.GetClientId();
            var result = await _paymentService.RegisterPaymentAsync(clientId, dto);
            return StatusCode(201, result);
        }

        /// <summary>SD-05: Consultar el Comprobante Digital de un Pago. GET /api/v1/payments/{id}/receipt</summary>
        [HttpGet("{id}/receipt")]
        [ProducesResponseType(typeof(PaymentReceiptDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetReceipt([FromRoute] Guid id)
        {
            var clientId = User.GetClientId();
            var result = await _paymentService.GetReceiptAsync(clientId, id);
            return Ok(result);
        }
    }
}

