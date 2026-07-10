using System;

namespace ToursAyacuchoPeruAPI.Domain.Entities
{
    public class NotificacionCliente
    {
        public Guid NotificacionId { get; set; }
        public string EventKey { get; set; } = null!;
        public string DestinatarioEmail { get; set; } = null!;
        public string Asunto { get; set; } = null!;
        public int Intentos { get; set; }
        public bool Entregada { get; set; }
        public string? UltimoError { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaEntrega { get; set; }
    }
}
