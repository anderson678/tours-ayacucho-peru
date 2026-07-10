// Tarea 4.2 â€” SD-09: Entidad Resena â€” TOURS AYACUCHO PERÃš
using System;

namespace ToursAyacuchoPeruAPI.Domain.Entities
{
    public class Resena
    {
        public Guid ResenaId { get; set; }
        public Guid UsuarioId { get; set; }
        public Guid PaqueteId { get; set; }

        /// <summary>
        /// CalificaciÃ³n del 1 al 5. RN-09-01, CHECK constraint en DDL.
        /// </summary>
        public int Calificacion { get; set; }

        /// <summary>
        /// Comentario opcional. MÃ¡ximo 1000 caracteres.
        /// </summary>
        public string? Comentario { get; set; }

        public DateTime FechaPublicacion { get; set; }

        // NavegaciÃ³n
        public Usuario Usuario { get; set; } = null!;
        public PaqueteTuristico Paquete { get; set; } = null!;
    }
}

