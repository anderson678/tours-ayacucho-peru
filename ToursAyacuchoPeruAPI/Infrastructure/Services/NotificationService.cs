using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ToursAyacuchoPeruAPI.Application.Interfaces;
using ToursAyacuchoPeruAPI.Domain.Entities;
using ToursAyacuchoPeruAPI.Infrastructure.Configuration;
using ToursAyacuchoPeruAPI.Infrastructure.Persistence;

namespace ToursAyacuchoPeruAPI.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private static readonly TimeSpan RetryDelay = TimeSpan.FromMinutes(5);
        private const int MaxAttempts = 3;

        private readonly SmtpSettings _settings;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IOptions<SmtpSettings> settings,
            IServiceScopeFactory scopeFactory,
            ILogger<NotificationService> logger)
        {
            _settings = settings.Value;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public Task SendWelcomeEmailAsync(string email, string name)
        {
            return SendOnceWithRetryAsync(
                eventKey: $"welcome:{email}",
                to: email,
                subject: "Bienvenido a TOURS AYACUCHO PERU",
                body: $"Hola {name}, tu cuenta fue creada exitosamente.");
        }

        public Task SendPaymentReceiptAsync(string email, string name, string receiptContentJson)
        {
            return SendOnceWithRetryAsync(
                eventKey: $"receipt:{email}:{StableHash(receiptContentJson)}",
                to: email,
                subject: "Comprobante digital - TOURS AYACUCHO PERU",
                body: $"Hola {name}, tu pago fue confirmado. Comprobante digital:\n\n{receiptContentJson}",
                onDelivered: db => MarkReceiptAsSentAsync(db, receiptContentJson));
        }

        public Task SendRescheduleConfirmationAsync(string email, string name, DateTime newDate)
        {
            return SendOnceWithRetryAsync(
                eventKey: $"reschedule:{email}:{newDate:O}",
                to: email,
                subject: "Reserva reprogramada - TOURS AYACUCHO PERU",
                body: $"Hola {name}, tu reserva fue reprogramada para el {newDate:yyyy-MM-dd HH:mm}.");
        }

        public Task SendTourReminderAsync(string email, string name, string packageName, DateTime startDate)
        {
            return SendOnceWithRetryAsync(
                eventKey: $"tour-reminder:{email}:{packageName}:{startDate:O}",
                to: email,
                subject: "Recordatorio de tour - TOURS AYACUCHO PERU",
                body: $"Hola {name}, te recordamos que tu tour {packageName} inicia el {startDate:yyyy-MM-dd HH:mm}.");
        }

        private async Task SendOnceWithRetryAsync(
            string eventKey,
            string to,
            string subject,
            string body,
            Func<ToursAyacuchoPeruDbContext, Task>? onDelivered = null)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ToursAyacuchoPeruDbContext>();
            var notification = await GetOrCreateNotificationAsync(db, eventKey, to, subject);

            if (notification.Entregada)
            {
                _logger.LogInformation("Notificacion omitida porque ya fue entregada. Evento: {EventKey}", eventKey);
                return;
            }

            for (var attempt = notification.Intentos + 1; attempt <= MaxAttempts; attempt++)
            {
                try
                {
                    await SendEmailAsync(to, subject, body);

                    notification.Intentos = attempt;
                    notification.Entregada = true;
                    notification.FechaEntrega = DateTime.UtcNow;
                    notification.UltimoError = null;

                    if (onDelivered != null)
                        await onDelivered(db);

                    await db.SaveChangesAsync();

                    _logger.LogInformation("Notificacion enviada. Evento: {EventKey}. Intento: {Attempt}", eventKey, attempt);
                    return;
                }
                catch (Exception ex)
                {
                    notification.Intentos = attempt;
                    notification.UltimoError = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
                    await db.SaveChangesAsync();

                    _logger.LogError(ex, "Fallo el envio de notificacion. Evento: {EventKey}. Intento: {Attempt}", eventKey, attempt);

                    if (attempt == MaxAttempts)
                        return;

                    await Task.Delay(RetryDelay);
                }
            }
        }

        private static async Task<NotificacionCliente> GetOrCreateNotificationAsync(
            ToursAyacuchoPeruDbContext db,
            string eventKey,
            string to,
            string subject)
        {
            var notification = await db.NotificacionesCliente
                .FirstOrDefaultAsync(n => n.EventKey == eventKey);

            if (notification != null)
                return notification;

            notification = new NotificacionCliente
            {
                NotificacionId = Guid.NewGuid(),
                EventKey = eventKey,
                DestinatarioEmail = to,
                Asunto = subject,
                Intentos = 0,
                Entregada = false,
                FechaCreacion = DateTime.UtcNow
            };

            db.NotificacionesCliente.Add(notification);
            try
            {
                await db.SaveChangesAsync();
                return notification;
            }
            catch (DbUpdateException)
            {
                db.Entry(notification).State = EntityState.Detached;
                return await db.NotificacionesCliente
                    .FirstAsync(n => n.EventKey == eventKey);
            }
        }

        private static async Task MarkReceiptAsSentAsync(ToursAyacuchoPeruDbContext db, string receiptContentJson)
        {
            var receipt = await db.Comprobantes
                .Where(c => c.Contenido == receiptContentJson)
                .OrderByDescending(c => c.FechaEmision)
                .FirstOrDefaultAsync();

            if (receipt != null)
                receipt.EnviadoCorreo = true;
        }

        private async Task SendEmailAsync(string to, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(_settings.Server)
                || string.IsNullOrWhiteSpace(_settings.SenderEmail)
                || _settings.Server.StartsWith("REEMPLAZAR", StringComparison.OrdinalIgnoreCase)
                || _settings.SenderEmail.StartsWith("REEMPLAZAR", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("SMTP no configurado. Simulando envio a {To}. Asunto: {Subject}", to, subject);
                return;
            }

            using var message = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = body
            };
            message.To.Add(to);

            using var client = new SmtpClient(_settings.Server, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl
            };

            if (!string.IsNullOrWhiteSpace(_settings.Username)
                && !_settings.Username.StartsWith("REEMPLAZAR", StringComparison.OrdinalIgnoreCase))
            {
                client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
            }

            await client.SendMailAsync(message);
        }

        private static string StableHash(string value)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            return Convert.ToHexString(bytes);
        }
    }
}
