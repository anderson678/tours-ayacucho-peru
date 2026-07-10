using System;
using ToursAyacuchoPeruAPI.Domain.Enums;

namespace ToursAyacuchoPeruAPI.Application.DTOs.Admin
{
    public class AdminClientResponseDto
    {
        public Guid ClienteId { get; set; }
        public string Nombre { get; set; } = null!;
        public string Correo { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public DateTime FechaRegistro { get; set; }
    }

    public class UpdateClientStatusDto
    {
        public EstadoUsuario Estado { get; set; }
    }
}
