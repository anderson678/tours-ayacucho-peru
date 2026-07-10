// Tarea 4.2 â€” SD-04, SD-06, SD-07: Entidad Reserva â€” TOURS AYACUCHO PERÃš
using System;
using ToursAyacuchoPeruAPI.Domain.Enums;

namespace ToursAyacuchoPeruAPI.Domain.Entities
{
    public class Reserva
    {
        public Guid ReservaId { get; set; }
        public Guid UsuarioId { get; set; }
        public Guid PaqueteId { get; set; }

        /// <summary>
        /// Fecha de inicio del tour reservado. RN-04: se usa ademÃ¡s para calcular la
        /// Ventana_de_ReprogramaciÃ³n (RN-06-01: mÃ­nimo 12 horas de anticipaciÃ³n).
        /// </summary>
        public DateTime FechaInicio { get; set; }

        /// <summary>
        /// Cantidad de asientos reservados. MÃ­nimo 1 (RN-04, CHECK en DDL).
        /// </summary>
        public int CantAsientos { get; set; }

        /// <summary>
        /// MontoTotal = PrecioUnitario Ã— CantAsientos. Calculado al momento de crear la reserva.
        /// DECIMAL(10,2).
        /// </summary>
        public decimal MontoTotal { get; set; }

        public EstadoReserva Estado { get; set; } = EstadoReserva.PENDIENTE_PAGO;

        /// <summary>
        /// RN-06-05: el nÃºmero mÃ¡ximo de reprogramaciones permitidas por Reserva es 1.
        /// </summary>
        public byte ContReprogramacion { get; set; } = 0;

        public DateTime FechaCreacion { get; set; }

        // NavegaciÃ³n
        public Usuario Usuario { get; set; } = null!;
        public PaqueteTuristico Paquete { get; set; } = null!;
        public Pago? Pago { get; set; }
    }
}

