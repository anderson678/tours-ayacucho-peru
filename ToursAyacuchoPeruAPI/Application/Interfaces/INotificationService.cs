// Tarea 14.1 â€” SD-11: Interfaz INotificationService â€” TOURS AYACUCHO PERÃš
// Cubre los 4 eventos definidos en el Requisito 11 (RN-11-01):
// registro de cuenta, pago confirmado, reprogramaciÃ³n confirmada y recordatorio de tour.
using System.Threading.Tasks;

namespace ToursAyacuchoPeruAPI.Application.Interfaces
{
    public interface INotificationService
    {
        /// <summary>RN-01-05 / RN-11-01: correo de bienvenida tras registro exitoso (â‰¤60s).</summary>
        Task SendWelcomeEmailAsync(string email, string name);

        /// <summary>RN-05-04 / RN-11-01: envÃ­o del Comprobante_Digital tras pago confirmado (â‰¤120s).</summary>
        Task SendPaymentReceiptAsync(string email, string name, string receiptContentJson);

        /// <summary>RN-06-06 / RN-11-01: confirmaciÃ³n de la nueva fecha tras reprogramaciÃ³n (â‰¤60s).</summary>
        Task SendRescheduleConfirmationAsync(string email, string name, System.DateTime newDate);

        /// <summary>RN-11-02: recordatorio 24 horas antes del inicio del tour.</summary>
        Task SendTourReminderAsync(string email, string name, string packageName, System.DateTime startDate);
    }
}

