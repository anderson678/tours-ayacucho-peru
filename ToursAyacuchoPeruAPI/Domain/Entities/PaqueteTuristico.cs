// Tarea 4.2 â€” SD-04, SD-10: Entidad PaqueteTuristico â€” TOURS AYACUCHO PERÃš
using System;
using System.Collections.Generic;

namespace ToursAyacuchoPeruAPI.Domain.Entities
{
    public class PaqueteTuristico
    {
        public Guid PaqueteId { get; set; }
        public string Nombre { get; set; } = null!;
        public string Destino { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string? ImagenUrl { get; set; }

        /// <summary>
        /// Precio base por persona. DECIMAL(10,2). RN-10-01: debe ser > 0.
        /// </summary>
        public decimal PrecioUnitario { get; set; }

        /// <summary>
        /// Capacidad total de asientos del paquete. RN-10-02: entero positivo.
        /// </summary>
        public int CapacidadTotal { get; set; }

        /// <summary>
        /// Asientos disponibles actualmente. Controlado con UPDLOCK/ROWLOCK en SD-04 y SD-06
        /// para prevenir overbooking bajo concurrencia.
        /// </summary>
        public int AsientosDisp { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; }

        // NavegaciÃ³n
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
        public ICollection<Resena> Resenas { get; set; } = new List<Resena>();
    }
}

