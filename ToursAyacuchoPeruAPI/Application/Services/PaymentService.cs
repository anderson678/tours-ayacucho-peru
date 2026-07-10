// Tarea 6.x â€” SD-05: Servicio PaymentService â€” TOURS AYACUCHO PERÃš
using System;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ToursAyacuchoPeruAPI.Infrastructure.Persistence;
using ToursAyacuchoPeruAPI.Domain.Entities;
using ToursAyacuchoPeruAPI.Domain.Enums;
using ToursAyacuchoPeruAPI.Domain.Exceptions;
using ToursAyacuchoPeruAPI.Application.DTOs.Reservations;
using ToursAyacuchoPeruAPI.Application.Interfaces;

namespace ToursAyacuchoPeruAPI.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ToursAyacuchoPeruDbContext _db;
        private readonly INotificationService _notificationService;

        public PaymentService(ToursAyacuchoPeruDbContext db, INotificationService notificationService)
        {
            _db = db;
            _notificationService = notificationService;
        }

        // RN-05-01 a RN-05-06: registro de pago + confirmaciÃ³n de reserva + comprobante digital,
        // todo dentro de una Ãºnica transacciÃ³n ACID.
        public async Task<PaymentResponseDto> RegisterPaymentAsync(Guid clientId, RegisterPaymentDto dto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            try
            {
                var reserva = await _db.Reservas
                    .FromSqlRaw(
                        "SELECT * FROM Reservas WITH (UPDLOCK, ROWLOCK) WHERE ReservaId = {0} AND UsuarioId = {1}",
                        dto.ReservaId,
                        clientId)
                    .Include(r => r.Paquete)
                    .Include(r => r.Usuario)
                    .FirstOrDefaultAsync();

                if (reserva == null)
                    throw new NotFoundException("Reserva no encontrada");

                // RN-05-06: una reserva CONFIRMADA no admite un segundo registro de pago
                if (reserva.Estado == EstadoReserva.CONFIRMADA)
                    throw new ConflictException("Esta reserva ya fue pagada", "RESERVA_YA_CONFIRMADA");

                if (reserva.Estado != EstadoReserva.PENDIENTE_PAGO)
                    throw new ConflictException(
                        "La reserva no se encuentra en un estado válido para registrar el pago",
                        "ESTADO_RESERVA_INVALIDO");

                // RN-05-02: tolerancia de S/ 0.01 por redondeo
                if (Math.Abs(dto.Monto - reserva.MontoTotal) > 0.01m)
                    throw new UnprocessableEntityException(
                        "El monto enviado no coincide con el monto esperado.",
                        new[]
                        {
                            $"Monto esperado: {reserva.MontoTotal:F2}",
                            $"Monto recibido: {dto.Monto:F2}"
                        },
                        "MONTO_INVALIDO");

                var pago = new Pago
                {
                    PagoId = Guid.NewGuid(),
                    ReservaId = reserva.ReservaId,
                    Monto = dto.Monto,
                    MetodoPago = dto.MetodoPago,
                    NumReferencia = dto.NumReferencia,
                    Estado = EstadoPago.Registrado,
                    FechaPago = DateTime.UtcNow
                };

                // RN-05-03: cambio de estado de la Reserva a CONFIRMADA dentro de la misma transacciÃ³n
                reserva.Estado = EstadoReserva.CONFIRMADA;

                _db.Pagos.Add(pago);
                _db.Reservas.Update(reserva);

                // RN-05-05: el Comprobante_Digital debe contener identificador de la Reserva,
                // nombre del Paquete, nombre del Cliente, monto, mÃ©todo de pago, nÃºmero de
                // referencia y fecha/hora del registro.
                var contenido = JsonSerializer.Serialize(new
                {
                    reservaId = reserva.ReservaId,
                    paquete = reserva.Paquete.Nombre,
                    cliente = reserva.Usuario.Nombre,
                    monto = pago.Monto,
                    metodoPago = pago.MetodoPago.ToString(),
                    numReferencia = pago.NumReferencia,
                    fechaHora = pago.FechaPago,
                    comprobanteAdjunto = string.IsNullOrWhiteSpace(dto.ComprobanteArchivoBase64)
                        ? null
                        : new
                        {
                            nombre = dto.ComprobanteArchivoNombre,
                            tipo = dto.ComprobanteArchivoTipo,
                            contenidoBase64 = dto.ComprobanteArchivoBase64
                        }
                });

                var comprobante = new Comprobante
                {
                    ComprobanteId = Guid.NewGuid(),
                    PagoId = pago.PagoId,
                    Contenido = contenido,
                    FechaEmision = DateTime.UtcNow,
                    EnviadoCorreo = false
                };

                _db.Comprobantes.Add(comprobante);

                try
                {
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
                {
                    throw new ConflictException("Esta reserva ya fue pagada", "RESERVA_YA_CONFIRMADA");
                }

                await transaction.CommitAsync();

                // RN-05-04: el Notification_Service debe enviar el comprobante en un mÃ¡ximo de 120s.
                // Se dispara fuera de la transacciÃ³n (no debe bloquear la respuesta HTTP al Cliente).
                _ = Task.Run(() =>
                    _notificationService.SendPaymentReceiptAsync(
                        reserva.Usuario.Correo, reserva.Usuario.Nombre, contenido));

                return new PaymentResponseDto
                {
                    PagoId = pago.PagoId,
                    ReservaId = pago.ReservaId,
                    Monto = pago.Monto,
                    Estado = pago.Estado.ToString(),
                    ReservaEstado = reserva.Estado.ToString(),
                    FechaPago = pago.FechaPago
                };
            }
            catch
            {
                try { await transaction.RollbackAsync(); } catch { /* la conexiÃ³n ya pudo cerrarse */ }
                throw;
            }
        }

        public async Task<PaymentReceiptDto> GetReceiptAsync(Guid clientId, Guid pagoId)
        {
            var pago = await _db.Pagos
                .Include(p => p.Reserva).ThenInclude(r => r.Paquete)
                .Include(p => p.Reserva).ThenInclude(r => r.Usuario)
                .Include(p => p.Comprobante)
                .FirstOrDefaultAsync(p => p.PagoId == pagoId && p.Reserva.UsuarioId == clientId);

            if (pago == null)
                throw new NotFoundException("Pago no encontrado");

            return new PaymentReceiptDto
            {
                PagoId = pago.PagoId,
                ReservaId = pago.ReservaId,
                Monto = pago.Monto,
                Estado = pago.Estado.ToString(),
                FechaPago = pago.FechaPago,
                ComprobanteContenido = pago.Comprobante?.Contenido,
                FechaEmision = pago.Comprobante?.FechaEmision,
                EnviadoCorreo = pago.Comprobante?.EnviadoCorreo ?? false
            };
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException exception)
        {
            return exception.InnerException is SqlException sqlException
                && (sqlException.Number == 2601 || sqlException.Number == 2627);
        }
    }
}

