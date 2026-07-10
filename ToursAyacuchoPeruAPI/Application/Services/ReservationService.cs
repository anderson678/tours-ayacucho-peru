// Tarea 5.x â€” SD-04, SD-07: Servicio ReservationService â€” TOURS AYACUCHO PERÃš
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
    public class ReservationService : IReservationService
    {
        private readonly ToursAyacuchoPeruDbContext _db;

        public ReservationService(ToursAyacuchoPeruDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<ReservationResponseDto>> GetByClientAsync(Guid clientId, string? estado = null)
        {
            var query = _db.Reservas
                .Include(r => r.Paquete)
                .Where(r => r.UsuarioId == clientId)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(estado))
            {
                if (!Enum.TryParse<EstadoReserva>(estado.Trim(), ignoreCase: true, out var estadoReserva))
                {
                    throw new UnprocessableEntityException(
                        "El estado de reserva proporcionado no es válido.",
                        new[] { "Estados permitidos: PENDIENTE_PAGO, CONFIRMADA, REPROGRAMADA, COMPLETADA, CANCELADA" },
                        "ESTADO_RESERVA_INVALIDO");
                }

                query = query.Where(r => r.Estado == estadoReserva);
            }

            var list = await query
                .OrderByDescending(r => r.FechaCreacion)
                .Select(r => new ReservationResponseDto
                {
                    ReservaId = r.ReservaId,
                    UsuarioId = r.UsuarioId,
                    PaqueteId = r.PaqueteId,
                    CantAsientos = r.CantAsientos,
                    MontoTotal = r.MontoTotal,
                    Estado = r.Estado.ToString(),
                    FechaInicio = r.FechaInicio,
                    PaqueteNombre = r.Paquete.Nombre,
                    PaqueteDestino = r.Paquete.Destino
                })
                .ToListAsync();

            return list;
        }

        public async Task<ReservationResponseDto> GetByIdAsync(Guid clientId, Guid reservaId)
        {
            var reserva = await _db.Reservas
                .Include(r => r.Paquete)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ReservaId == reservaId && r.UsuarioId == clientId);

            if (reserva == null)
                throw new NotFoundException("Reserva no encontrada");

            return new ReservationResponseDto
            {
                ReservaId = reserva.ReservaId,
                UsuarioId = reserva.UsuarioId,
                PaqueteId = reserva.PaqueteId,
                CantAsientos = reserva.CantAsientos,
                MontoTotal = reserva.MontoTotal,
                Estado = reserva.Estado.ToString(),
                FechaInicio = reserva.FechaInicio,
                PaqueteNombre = reserva.Paquete.Nombre,
                PaqueteDestino = reserva.Paquete.Destino
            };
        }

        public async Task<IEnumerable<AdminReservationResponseDto>> GetAllForAdminAsync(string? estado = null, Guid? paqueteId = null)
        {
            var query = _db.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Paquete)
                .Include(r => r.Pago)
                    .ThenInclude(p => p!.Comprobante)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(estado))
            {
                if (!Enum.TryParse<EstadoReserva>(estado.Trim(), ignoreCase: true, out var estadoReserva))
                {
                    throw new UnprocessableEntityException(
                        "El estado de reserva proporcionado no es válido.",
                        new[] { "Estados permitidos: PENDIENTE_PAGO, CONFIRMADA, REPROGRAMADA, COMPLETADA, CANCELADA" },
                        "ESTADO_RESERVA_INVALIDO");
                }

                query = query.Where(r => r.Estado == estadoReserva);
            }

            if (paqueteId.HasValue)
            {
                query = query.Where(r => r.PaqueteId == paqueteId.Value);
            }

            var reservas = await query
                .OrderByDescending(r => r.FechaCreacion)
                .ToListAsync();

            return reservas.Select(r => new AdminReservationResponseDto
                {
                    ReservaId = r.ReservaId,
                    UsuarioId = r.UsuarioId,
                    ClienteNombre = r.Usuario.Nombre,
                    ClienteCorreo = r.Usuario.Correo,
                    ClienteTelefono = r.Usuario.Telefono,
                    PaqueteId = r.PaqueteId,
                    PaqueteNombre = r.Paquete.Nombre,
                    PaqueteDestino = r.Paquete.Destino,
                    CantAsientos = r.CantAsientos,
                    MontoTotal = r.MontoTotal,
                    Estado = r.Estado.ToString(),
                    FechaInicio = r.FechaInicio,
                    FechaCreacion = r.FechaCreacion,
                    PagoEstado = r.Pago != null ? r.Pago.Estado.ToString() : "Sin pago",
                    FechaPago = r.Pago != null ? r.Pago.FechaPago : null,
                    MetodoPago = r.Pago != null ? r.Pago.MetodoPago.ToString() : null,
                    NumReferencia = r.Pago != null ? r.Pago.NumReferencia : null,
                    ComprobanteArchivoNombre = GetReceiptFileName(r.Pago?.Comprobante?.Contenido)
                })
                .ToList();
        }

        // RN-04-01 a RN-04-05: transacciÃ³n ACID con bloqueo pesimista (UPDLOCK/ROWLOCK)
        // para prevenir overbooking bajo solicitudes concurrentes.
        public async Task<ReservationResponseDto> CreateAsync(Guid clientId, CreateReservationDto dto)
        {
            if (dto.CantAsientos < 1)
                throw new UnprocessableEntityException("La cantidad de asientos solicitados debe ser al menos 1.");

            using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                var paquete = await _db.PaquetesTuristicos
                    .FromSqlRaw("SELECT * FROM PaquetesTuristicos WITH (UPDLOCK, ROWLOCK) WHERE PaqueteId = {0}", dto.PaqueteId)
                    .FirstOrDefaultAsync();

                if (paquete == null || !paquete.Activo)
                    throw new NotFoundException("Paquete no encontrado");

                // RN-04-02: no confirmar si los asientos solicitados superan la disponibilidad
                if (paquete.AsientosDisp < dto.CantAsientos)
                    throw new ConflictException(
                        $"Asientos insuficientes. Disponibles: {paquete.AsientosDisp}",
                        "ASIENTOS_INSUFICIENTES");

                if (dto.FechaInicio.Date != paquete.FechaInicio.Date)
                    throw new UnprocessableEntityException(
                        "La fecha de inicio solicitada no coincide con la fecha disponible del paquete turístico.",
                        new[] { $"Fecha disponible: {paquete.FechaInicio:yyyy-MM-dd}" },
                        "FECHA_INICIO_INVALIDA");

                // RN-04-05: un cliente no puede tener mÃ¡s de una reserva PENDIENTE_PAGO
                // para el mismo paquete (validaciÃ³n de aplicaciÃ³n + Ã­ndice Ãºnico filtrado en BD,
                // ver database/ToursAyacuchoPeru.sql, como defensa adicional ante condiciones de carrera).
                var reservaPendiente = await _db.Reservas
                    .Where(r => r.UsuarioId == clientId
                             && r.PaqueteId == dto.PaqueteId
                             && r.Estado == EstadoReserva.PENDIENTE_PAGO)
                    .FirstOrDefaultAsync();

                if (reservaPendiente != null)
                    throw new ConflictException(
                        "Ya tienes una reserva pendiente de pago para este paquete",
                        "RESERVA_PENDIENTE_DUPLICADA");

                var reserva = new Reserva
                {
                    ReservaId = Guid.NewGuid(),
                    UsuarioId = clientId,
                    PaqueteId = dto.PaqueteId,
                    CantAsientos = dto.CantAsientos,
                    MontoTotal = paquete.PrecioUnitario * dto.CantAsientos,
                    Estado = EstadoReserva.PENDIENTE_PAGO,
                    FechaInicio = dto.FechaInicio,
                    FechaCreacion = DateTime.UtcNow
                };

                // RN-04-03: descuento de asientos atÃ³mico junto con la creaciÃ³n de la reserva
                paquete.AsientosDisp -= dto.CantAsientos;

                _db.Reservas.Add(reserva);
                _db.PaquetesTuristicos.Update(paquete);

                try
                {
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
                {
                    throw new ConflictException(
                        "Ya tienes una reserva pendiente de pago para este paquete",
                        "RESERVA_PENDIENTE_DUPLICADA");
                }

                await transaction.CommitAsync();

                return new ReservationResponseDto
                {
                    ReservaId = reserva.ReservaId,
                    UsuarioId = reserva.UsuarioId,
                    PaqueteId = reserva.PaqueteId,
                    CantAsientos = reserva.CantAsientos,
                    MontoTotal = reserva.MontoTotal,
                    Estado = reserva.Estado.ToString(),
                    FechaInicio = reserva.FechaInicio,
                    PaqueteNombre = paquete.Nombre,
                    PaqueteDestino = paquete.Destino
                };
            }
            catch
            {
                try { await transaction.RollbackAsync(); } catch { /* la conexiÃ³n ya pudo cerrarse */ }
                throw;
            }
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException exception)
        {
            return exception.InnerException is SqlException sqlException
                && (sqlException.Number == 2601 || sqlException.Number == 2627);
        }

        private static string? GetReceiptFileName(string? contenido)
        {
            if (string.IsNullOrWhiteSpace(contenido)) return null;

            try
            {
                using var document = JsonDocument.Parse(contenido);
                if (document.RootElement.TryGetProperty("comprobanteAdjunto", out var adjunto)
                    && adjunto.ValueKind == JsonValueKind.Object
                    && adjunto.TryGetProperty("nombre", out var nombre))
                {
                    return nombre.GetString();
                }
            }
            catch (JsonException)
            {
                return null;
            }

            return null;
        }
    }
}


