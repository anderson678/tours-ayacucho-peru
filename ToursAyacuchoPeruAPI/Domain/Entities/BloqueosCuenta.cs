// Tarea 1.2 â€” SD-02: Entidad BloqueosCuenta â€” TOURS AYACUCHO PERÃš
using System;

namespace ToursAyacuchoPeruAPI.Domain.Entities
{
    public class BloqueosCuenta
    {
        public int BloqueoId { get; set; }
        public Guid UsuarioId { get; set; }
        public byte IntentosFallidos { get; set; }
        public DateTime? FechaBloqueo { get; set; }
        public DateTime? FechaDesbloqueo { get; set; }

        // Propiedad de navegaciÃ³n
        public virtual Usuario Usuario { get; set; } = null!;
    }
}

