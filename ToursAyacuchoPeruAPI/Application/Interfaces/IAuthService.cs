// Tarea 2.6, 3.1, 3.3 â€” SD-01, SD-02, SD-03: Interfaz IAuthService â€” TOURS AYACUCHO PERÃš
using System;
using System.Threading.Tasks;
using ToursAyacuchoPeruAPI.Application.DTOs.Auth;

namespace ToursAyacuchoPeruAPI.Application.Interfaces
{
    public interface IAuthService
    {
        Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto dto);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);
        Task<UpdateProfileResponseDto> GetProfileAsync(Guid clientId);
        Task<UpdateProfileResponseDto> UpdateProfileAsync(Guid clientId, UpdateProfileDto dto);
    }
}

