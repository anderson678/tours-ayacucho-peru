using ToursAyacuchoPeruAPI.Application.Services;
using ToursAyacuchoPeruAPI.Domain.Entities;
using ToursAyacuchoPeruAPI.Domain.Enums;
using ToursAyacuchoPeruAPI.Domain.Exceptions;
using ToursAyacuchoPeruAPI.Tests.TestSupport;

namespace ToursAyacuchoPeruAPI.Tests;

public class PaymentServiceTests
{
    [Fact]
    public async Task GetReceiptAsync_returns_receipt_for_owner()
    {
        await using var db = TestDb.Create();
        var client = CreateUser();
        var package = CreatePackage();
        var reservation = CreateReservation(client.UsuarioId, package.PaqueteId);
        var payment = CreatePayment(reservation.ReservaId);
        var receipt = new Comprobante
        {
            ComprobanteId = Guid.NewGuid(),
            PagoId = payment.PagoId,
            Contenido = """{"reservaId":"demo","paquete":"Millpu"}""",
            FechaEmision = DateTime.UtcNow,
            EnviadoCorreo = true
        };

        db.Usuarios.Add(client);
        db.PaquetesTuristicos.Add(package);
        db.Reservas.Add(reservation);
        db.Pagos.Add(payment);
        db.Comprobantes.Add(receipt);
        await db.SaveChangesAsync();
        var service = new PaymentService(db, new FakeNotificationService());

        var result = await service.GetReceiptAsync(client.UsuarioId, payment.PagoId);

        Assert.Equal(payment.PagoId, result.PagoId);
        Assert.Equal(240m, result.Monto);
        Assert.True(result.EnviadoCorreo);
        Assert.Contains("Millpu", result.ComprobanteContenido);
    }

    [Fact]
    public async Task GetReceiptAsync_rejects_payment_from_another_client()
    {
        await using var db = TestDb.Create();
        var client = CreateUser();
        var package = CreatePackage();
        var reservation = CreateReservation(client.UsuarioId, package.PaqueteId);
        var payment = CreatePayment(reservation.ReservaId);

        db.Usuarios.Add(client);
        db.PaquetesTuristicos.Add(package);
        db.Reservas.Add(reservation);
        db.Pagos.Add(payment);
        await db.SaveChangesAsync();
        var service = new PaymentService(db, new FakeNotificationService());

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetReceiptAsync(Guid.NewGuid(), payment.PagoId));
    }

    private static Usuario CreateUser()
    {
        return new Usuario
        {
            UsuarioId = Guid.NewGuid(),
            Nombre = "Cliente Demo",
            Correo = "cliente@email.com",
            HashPassword = BCrypt.Net.BCrypt.HashPassword("Correcta@2026"),
            Telefono = "987654321",
            Rol = RolUsuario.Cliente,
            Estado = EstadoUsuario.Activo,
            FechaRegistro = DateTime.UtcNow
        };
    }

    private static PaqueteTuristico CreatePackage()
    {
        return new PaqueteTuristico
        {
            PaqueteId = Guid.NewGuid(),
            Nombre = "Millpu Full Day",
            Destino = "Huancaraylla",
            PrecioUnitario = 120m,
            CapacidadTotal = 20,
            AsientosDisp = 10,
            FechaInicio = DateTime.UtcNow.Date.AddDays(10),
            FechaFin = DateTime.UtcNow.Date.AddDays(10),
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };
    }

    private static Reserva CreateReservation(Guid userId, Guid packageId)
    {
        return new Reserva
        {
            ReservaId = Guid.NewGuid(),
            UsuarioId = userId,
            PaqueteId = packageId,
            CantAsientos = 2,
            MontoTotal = 240m,
            Estado = EstadoReserva.CONFIRMADA,
            FechaInicio = DateTime.UtcNow.Date.AddDays(10),
            FechaCreacion = DateTime.UtcNow
        };
    }

    private static Pago CreatePayment(Guid reservationId)
    {
        return new Pago
        {
            PagoId = Guid.NewGuid(),
            ReservaId = reservationId,
            Monto = 240m,
            MetodoPago = MetodoPago.Yape,
            NumReferencia = "YAPE-123",
            Estado = EstadoPago.Registrado,
            FechaPago = DateTime.UtcNow
        };
    }
}
