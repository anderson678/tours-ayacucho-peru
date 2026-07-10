// Tarea 3.3 â€” SD-02: DTO LoginResponseDto â€” TOURS AYACUCHO PERÃš
using System;

namespace ToursAyacuchoPeruAPI.Application.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = null!;
        public DateTime ExpiraEn { get; set; }
        public Guid ClienteId { get; set; }
        public string Rol { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string Correo { get; set; } = null!;
        public string Telefono { get; set; } = null!;
        public string? FotoUrl { get; set; }
    }
}

