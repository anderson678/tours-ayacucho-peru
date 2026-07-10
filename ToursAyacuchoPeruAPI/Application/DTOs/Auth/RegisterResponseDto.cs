// Tarea 2.2 â€” SD-01: DTO RegisterResponseDto â€” TOURS AYACUCHO PERÃš
using System;

namespace ToursAyacuchoPeruAPI.Application.DTOs.Auth
{
    public class RegisterResponseDto
    {
        public Guid ClienteId { get; set; }
        public string Mensaje { get; set; } = null!;
    }
}

