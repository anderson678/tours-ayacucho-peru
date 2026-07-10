// Tarea 7.x â€” SD-06: Interfaz IReschedulingService â€” TOURS AYACUCHO PERÃš
using System;
using System.Threading.Tasks;
using ToursAyacuchoPeruAPI.Application.DTOs.Reservations;

namespace ToursAyacuchoPeruAPI.Application.Interfaces
{
    public interface IReschedulingService
    {
        Task<ReservationResponseDto> RescheduleAsync(Guid clientId, Guid reservaId, RescheduleRequestDto dto);
    }
}

