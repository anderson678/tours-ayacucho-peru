// Tarea 2.2 â€” SD-01: DTO RegisterRequestDto â€” TOURS AYACUCHO PERÃš
using System.ComponentModel.DataAnnotations;

namespace ToursAyacuchoPeruAPI.Application.DTOs.Auth
{
    public class RegisterRequestDto
    {
        [Required]
        public string Nombre { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Correo { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        [Required]
        public string Telefono { get; set; } = null!;
    }
}

