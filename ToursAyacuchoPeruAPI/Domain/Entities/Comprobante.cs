// Tarea 4.2 â€” SD-05: Entidad Comprobante â€” TOURS AYACUCHO PERÃš
using System;

namespace ToursAyacuchoPeruAPI.Domain.Entities
{
    public class Comprobante
    {
        public Guid ComprobanteId { get; set; }
        public Guid PagoId { get; set; }

        /// <summary>
        /// Contenido del comprobante en JSON estructurado. Debe incluir (RN-05-05):
        /// identificador de la Reserva, nombre del Paquete, nombre del Cliente, monto pagado,
        /// mÃ©todo de pago, nÃºmero de referencia y fecha/hora del registro.
        /// </summary>
        public string Contenido { get; set; } = null!;

        public DateTime FechaEmision { get; set; }

        /// <summary>
        /// RN-05-04: indica si el Notification_Service ya enviÃ³ el comprobante por correo.
        /// </summary>
        public bool EnviadoCorreo { get; set; }

        // NavegaciÃ³n
        public Pago Pago { get; set; } = null!;
    }
}

