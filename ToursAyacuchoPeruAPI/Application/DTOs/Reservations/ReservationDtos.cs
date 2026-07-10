using System;
using ToursAyacuchoPeruAPI.Domain.Enums;

namespace ToursAyacuchoPeruAPI.Application.DTOs.Reservations
{
    public class CreateReservationDto
    {
        public Guid PaqueteId { get; set; }
        public int CantAsientos { get; set; }
        public DateTime FechaInicio { get; set; }
    }

    public class ReservationResponseDto
    {
        public Guid ReservaId { get; set; }
        public Guid UsuarioId { get; set; }
        public Guid PaqueteId { get; set; }
        public int CantAsientos { get; set; }
        public decimal MontoTotal { get; set; }
        public string Estado { get; set; } = null!;
        public DateTime FechaInicio { get; set; }
        public string? PaqueteNombre { get; set; }
        public string? PaqueteDestino { get; set; }
    }

    public class AdminReservationResponseDto
    {
        public Guid ReservaId { get; set; }
        public Guid UsuarioId { get; set; }
        public string ClienteNombre { get; set; } = null!;
        public string ClienteCorreo { get; set; } = null!;
        public string? ClienteTelefono { get; set; }
        public Guid PaqueteId { get; set; }
        public string PaqueteNombre { get; set; } = null!;
        public string PaqueteDestino { get; set; } = null!;
        public int CantAsientos { get; set; }
        public decimal MontoTotal { get; set; }
        public string Estado { get; set; } = null!;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string PagoEstado { get; set; } = "Sin pago";
        public DateTime? FechaPago { get; set; }
        public string? MetodoPago { get; set; }
        public string? NumReferencia { get; set; }
        public string? ComprobanteArchivoNombre { get; set; }
    }

    public class RegisterPaymentDto
    {
        public Guid ReservaId { get; set; }
        public decimal Monto { get; set; }
        public MetodoPago MetodoPago { get; set; }
        public string NumReferencia { get; set; } = null!;
        public string? ComprobanteArchivoNombre { get; set; }
        public string? ComprobanteArchivoTipo { get; set; }
        public string? ComprobanteArchivoBase64 { get; set; }
    }

    public class PaymentResponseDto
    {
        public Guid PagoId { get; set; }
        public Guid ReservaId { get; set; }
        public decimal Monto { get; set; }
        public string Estado { get; set; } = null!;
        public string ReservaEstado { get; set; } = null!;
        public DateTime FechaPago { get; set; }
    }

    public class RescheduleRequestDto
    {
        public DateTime NuevaFecha { get; set; }
    }

    public class PaymentReceiptDto
    {
        public Guid PagoId { get; set; }
        public Guid ReservaId { get; set; }
        public decimal Monto { get; set; }
        public string Estado { get; set; } = null!;
        public DateTime FechaPago { get; set; }
        public string? ComprobanteContenido { get; set; }
        public DateTime? FechaEmision { get; set; }
        public bool EnviadoCorreo { get; set; }
    }
}

