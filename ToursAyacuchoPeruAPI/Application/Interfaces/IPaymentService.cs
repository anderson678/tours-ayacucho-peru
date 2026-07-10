// Tarea 6.x â€” SD-05: Interfaz IPaymentService â€” TOURS AYACUCHO PERÃš
using System;
using System.Threading.Tasks;
using ToursAyacuchoPeruAPI.Application.DTOs.Reservations;

namespace ToursAyacuchoPeruAPI.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto> RegisterPaymentAsync(Guid clientId, RegisterPaymentDto dto);
        Task<PaymentReceiptDto> GetReceiptAsync(Guid clientId, Guid pagoId);
    }
}

