// Tarea 5.x â€” SD-04, SD-07: Interfaz IReservationService â€” TOURS AYACUCHO PERÃš
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToursAyacuchoPeruAPI.Application.DTOs.Reservations;

namespace ToursAyacuchoPeruAPI.Application.Interfaces
{
    public interface IReservationService
    {
        Task<ReservationResponseDto> CreateAsync(Guid clientId, CreateReservationDto dto);
        Task<IEnumerable<ReservationResponseDto>> GetByClientAsync(Guid clientId, string? estado = null);
        Task<ReservationResponseDto> GetByIdAsync(Guid clientId, Guid reservaId);
        Task<IEnumerable<AdminReservationResponseDto>> GetAllForAdminAsync(string? estado = null, Guid? paqueteId = null);
    }
}

