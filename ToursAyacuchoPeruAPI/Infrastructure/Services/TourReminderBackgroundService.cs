using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ToursAyacuchoPeruAPI.Application.Interfaces;
using ToursAyacuchoPeruAPI.Domain.Enums;
using ToursAyacuchoPeruAPI.Infrastructure.Persistence;

namespace ToursAyacuchoPeruAPI.Infrastructure.Services
{
    public class TourReminderBackgroundService : BackgroundService
    {
        private static readonly TimeSpan ScanInterval = TimeSpan.FromHours(1);
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TourReminderBackgroundService> _logger;

        public TourReminderBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<TourReminderBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SendUpcomingTourRemindersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al procesar recordatorios de tour.");
                }

                await Task.Delay(ScanInterval, stoppingToken);
            }
        }

        private async Task SendUpcomingTourRemindersAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ToursAyacuchoPeruDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var now = DateTime.UtcNow;
            var windowStart = now.AddHours(23);
            var windowEnd = now.AddHours(24);

            var reservations = await db.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Paquete)
                .Where(r => r.Estado == EstadoReserva.CONFIRMADA
                    && r.FechaInicio >= windowStart
                    && r.FechaInicio <= windowEnd)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var reservation in reservations)
            {
                await notificationService.SendTourReminderAsync(
                    reservation.Usuario.Correo,
                    reservation.Usuario.Nombre,
                    reservation.Paquete.Nombre,
                    reservation.FechaInicio);
            }
        }
    }
}
