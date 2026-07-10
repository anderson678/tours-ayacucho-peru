// Tarea 7.x â€” SD-06: Servicio ReschedulingService â€” TOURS AYACUCHO PERÃš
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ToursAyacuchoPeruAPI.Infrastructure.Persistence;
using ToursAyacuchoPeruAPI.Domain.Entities;
using ToursAyacuchoPeruAPI.Domain.Enums;
using ToursAyacuchoPeruAPI.Domain.Exceptions;
using ToursAyacuchoPeruAPI.Application.DTOs.Reservations;
using ToursAyacuchoPeruAPI.Application.Interfaces;

namespace ToursAyacuchoPeruAPI.Application.Services
{
    public class ReschedulingService : IReschedulingService
    {
        private readonly ToursAyacuchoPeruDbContext _db;
        private readonly INotificationService _notificationService;

        public ReschedulingService(ToursAyacuchoPeruDbContext db, INotificationService notificationService)
        {
            _db = db;
            _notificationService = notificationService;
        }

        // RN-06-01 a RN-06-06: reprogramación transaccional con notificación al Cliente.
        // Modelo MVP: cada PaqueteTuristico representa una salida con disponibilidad global.
        // Los asientos ya fueron descontados al crear la reserva; reprogramar la misma reserva
        // no debe volver a descontarlos.
        public async Task<ReservationResponseDto> RescheduleAsync(Guid clientId, Guid reservaId, RescheduleRequestDto dto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                var reserva = await _db.Reservas
                    .FromSqlRaw(
                        "SELECT * FROM Reservas WITH (UPDLOCK, ROWLOCK) WHERE ReservaId = {0} AND UsuarioId = {1}",
                        reservaId,
                        clientId)
                    .Include(r => r.Paquete)
                    .Include(r => r.Usuario)
                    .FirstOrDefaultAsync();

                if (reserva == null)
                    throw new NotFoundException("Reserva no encontrada");

                if (reserva.Estado != EstadoReserva.CONFIRMADA)
                    throw new ConflictException(
                        "Solo reservas confirmadas pueden reprogramarse",
                        "RESERVA_NO_CONFIRMADA");

                // RN-06-05: mÃ¡ximo 1 reprogramaciÃ³n permitida por reserva
                if (reserva.ContReprogramacion >= 1)
                    throw new UnprocessableEntityException(
                        "Límite de reprogramaciones alcanzado (máximo 1 por reserva)",
                        "LIMITE_REPROGRAMACIONES");

                // RN-06-01: mÃ­nimo 12 horas de anticipaciÃ³n a la fecha de inicio ORIGINAL
                var hoursUntil = (reserva.FechaInicio - DateTime.UtcNow).TotalHours;
                if (hoursUntil < 12)
                    throw new UnprocessableEntityException(
                        "Debe solicitarse con al menos 12 horas de anticipación",
                        "FUERA_DE_VENTANA");

                // RN-06-02: la nueva fecha debe ser futura
                if (dto.NuevaFecha <= DateTime.UtcNow)
                    throw new UnprocessableEntityException(
                        "La nueva fecha debe ser futura",
                        "NUEVA_FECHA_INVALIDA");

                var paquete = await _db.PaquetesTuristicos
                    .FromSqlRaw("SELECT * FROM PaquetesTuristicos WITH (UPDLOCK, ROWLOCK) WHERE PaqueteId = {0}", reserva.PaqueteId)
                    .FirstOrDefaultAsync();

                if (paquete == null)
                    throw new NotFoundException("Paquete no encontrado");

                // RN-06-04: la actualización de fecha, estado y contador ocurre en una sola TX.
                // En este modelo no se muta AsientosDisp: la reserva conserva los asientos ya retenidos.
                reserva.FechaInicio = dto.NuevaFecha;
                reserva.Estado = EstadoReserva.REPROGRAMADA;
                reserva.ContReprogramacion += 1;

                _db.Reservas.Update(reserva);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                // RN-06-06: notificar al Cliente la confirmaciÃ³n de la reprogramaciÃ³n (â‰¤ 60s)
                _ = Task.Run(() =>
                    _notificationService.SendRescheduleConfirmationAsync(
                        reserva.Usuario.Correo, reserva.Usuario.Nombre, reserva.FechaInicio));

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
            catch
            {
                try { await transaction.RollbackAsync(); } catch { /* la conexiÃ³n ya pudo cerrarse */ }
                throw;
            }
        }
    }
}

