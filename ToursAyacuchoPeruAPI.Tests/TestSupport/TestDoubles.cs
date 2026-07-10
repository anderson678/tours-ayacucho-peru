using ToursAyacuchoPeruAPI.Application.Interfaces;

namespace ToursAyacuchoPeruAPI.Tests.TestSupport;

internal sealed class FakeJwtService : IJwtService
{
    public List<(Guid ClientId, string Rol)> Calls { get; } = new();

    public (string Token, DateTime ExpiresAt) GenerateToken(Guid clientId, string rol)
    {
        Calls.Add((clientId, rol));
        return ($"fake-token-{clientId:N}-{rol}", DateTime.UtcNow.AddHours(8));
    }
}

internal sealed class FakeNotificationService : INotificationService
{
    public List<(string Email, string Name)> WelcomeEmails { get; } = new();
    public List<(string Email, string Name, string Receipt)> PaymentReceipts { get; } = new();
    public List<(string Email, string Name, DateTime NewDate)> Reschedules { get; } = new();
    public List<(string Email, string Name, string PackageName, DateTime StartDate)> Reminders { get; } = new();

    public Task SendWelcomeEmailAsync(string email, string name)
    {
        WelcomeEmails.Add((email, name));
        return Task.CompletedTask;
    }

    public Task SendPaymentReceiptAsync(string email, string name, string receiptContentJson)
    {
        PaymentReceipts.Add((email, name, receiptContentJson));
        return Task.CompletedTask;
    }

    public Task SendRescheduleConfirmationAsync(string email, string name, DateTime newDate)
    {
        Reschedules.Add((email, name, newDate));
        return Task.CompletedTask;
    }

    public Task SendTourReminderAsync(string email, string name, string packageName, DateTime startDate)
    {
        Reminders.Add((email, name, packageName, startDate));
        return Task.CompletedTask;
    }
}
