// Tarea 3.3 â€” SD-02: DTO LoginRequestDto â€” TOURS AYACUCHO PERÃš
using System.ComponentModel.DataAnnotations;

namespace ToursAyacuchoPeruAPI.Application.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Correo { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}

