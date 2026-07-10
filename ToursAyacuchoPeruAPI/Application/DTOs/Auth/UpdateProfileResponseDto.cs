using System;

namespace ToursAyacuchoPeruAPI.Application.DTOs.Auth
{
    public class UpdateProfileResponseDto
    {
        public Guid ClienteId { get; set; }
        public string Nombre { get; set; } = null!;
        public string Correo { get; set; } = null!;
        public string Telefono { get; set; } = null!;
        public string? FotoUrl { get; set; }
        public string Rol { get; set; } = null!;
    }
}
