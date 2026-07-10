// Tarea 4.2 â€” SD-05: Entidad Pago â€” TOURS AYACUCHO PERÃš
using System;
using ToursAyacuchoPeruAPI.Domain.Enums;

namespace ToursAyacuchoPeruAPI.Domain.Entities
{
    public class Pago
    {
        public Guid PagoId { get; set; }
        public Guid ReservaId { get; set; }

        /// <summary>
        /// Monto pagado. DECIMAL(10,2). Debe coincidir con Reserva.MontoTotal (tolerancia S/ 0.01, RN-05-02).
        /// </summary>
        public decimal Monto { get; set; }

        public MetodoPago MetodoPago { get; set; }

        /// <summary>
        /// NÃºmero de referencia del comprobante de pago externo (RN-05-01/02).
        /// </summary>
        public string NumReferencia { get; set; } = null!;

        public EstadoPago Estado { get; set; } = EstadoPago.Registrado;
        public DateTime FechaPago { get; set; }

        // NavegaciÃ³n
        public Reserva Reserva { get; set; } = null!;
        public Comprobante? Comprobante { get; set; }
    }
}

