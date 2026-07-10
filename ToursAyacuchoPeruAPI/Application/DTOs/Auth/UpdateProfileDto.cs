// Tarea 3.3 â€” SD-03: DTO UpdateProfileDto â€” TOURS AYACUCHO PERÃš
namespace ToursAyacuchoPeruAPI.Application.DTOs.Auth
{
    public class UpdateProfileDto
    {
        public string? Nombre { get; set; }
        public string? Telefono { get; set; }
        public string? FotoUrl { get; set; }

        // RN-03-01: se acepta para compatibilidad con clientes que lo envíen,
        // pero AuthService lo ignora y nunca lo persiste.
        public string? Correo { get; set; }
    }
}

