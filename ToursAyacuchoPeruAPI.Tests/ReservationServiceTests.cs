using ToursAyacuchoPeruAPI.Application.Services;
using ToursAyacuchoPeruAPI.Domain.Entities;
using ToursAyacuchoPeruAPI.Domain.Enums;
using ToursAyacuchoPeruAPI.Domain.Exceptions;
using ToursAyacuchoPeruAPI.Tests.TestSupport;

namespace ToursAyacuchoPeruAPI.Tests;

public class ReservationServiceTests
{
    [Fact]
    public async Task GetByClientAsync_returns_only_client_reservations()
    {
        await using var db = TestDb.Create();
        var clientId = Guid.NewGuid();
        var otherClientId = Guid.NewGuid();
        var package = CreatePackage();
        db.PaquetesTuristicos.Add(package);
        db.Reservas.Add(CreateReservation(clientId, package.PaqueteId, EstadoReserva.PENDIENTE_PAGO));
        db.Reservas.Add(CreateReservation(otherClientId, package.PaqueteId, EstadoReserva.CONFIRMADA));
        await db.SaveChangesAsync();
        var service = new ReservationService(db);

        var result = (await service.GetByClientAsync(clientId)).ToList();

        Assert.Single(result);
        Assert.Equal(clientId, result[0].UsuarioId);
        Assert.Equal("PENDIENTE_PAGO", result[0].Estado);
    }

    [Fact]
    public async Task GetByClientAsync_filters_by_status()
    {
        await using var db = TestDb.Create();
        var clientId = Guid.NewGuid();
        var package = CreatePackage();
        db.PaquetesTuristicos.Add(package);
        db.Reservas.Add(CreateReservation(clientId, package.PaqueteId, EstadoReserva.PENDIENTE_PAGO));
        db.Reservas.Add(CreateReservation(clientId, package.PaqueteId, EstadoReserva.CONFIRMADA));
        await db.SaveChangesAsync();
        var service = new ReservationService(db);

        var result = (await service.GetByClientAsync(clientId, "Confirmada")).ToList();

        Assert.Single(result);
        Assert.Equal("CONFIRMADA", result[0].Estado);
    }

    [Fact]
    public async Task GetByClientAsync_rejects_invalid_status()
    {
        await using var db = TestDb.Create();
        var service = new ReservationService(db);

        await Assert.ThrowsAsync<UnprocessableEntityException>(
            () => service.GetByClientAsync(Guid.NewGuid(), "NO_EXISTE"));
    }

    [Fact]
    public async Task GetByIdAsync_rejects_reservation_from_another_client()
    {
        await using var db = TestDb.Create();
        var ownerId = Guid.NewGuid();
        var package = CreatePackage();
        var reservation = CreateReservation(ownerId, package.PaqueteId, EstadoReserva.CONFIRMADA);
        db.PaquetesTuristicos.Add(package);
        db.Reservas.Add(reservation);
        await db.SaveChangesAsync();
        var service = new ReservationService(db);

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetByIdAsync(Guid.NewGuid(), reservation.ReservaId));
    }

    private static PaqueteTuristico CreatePackage()
    {
        return new PaqueteTuristico
        {
            PaqueteId = Guid.NewGuid(),
            Nombre = "Millpu Full Day",
            Destino = "Huancaraylla",
            Descripcion = "Aguas turquesas",
            PrecioUnitario = 120m,
            CapacidadTotal = 20,
            AsientosDisp = 10,
            FechaInicio = DateTime.UtcNow.Date.AddDays(10),
            FechaFin = DateTime.UtcNow.Date.AddDays(10),
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };
    }

    private static Reserva CreateReservation(Guid userId, Guid packageId, EstadoReserva estado)
    {
        return new Reserva
        {
            ReservaId = Guid.NewGuid(),
            UsuarioId = userId,
            PaqueteId = packageId,
            CantAsientos = 2,
            MontoTotal = 240m,
            Estado = estado,
            FechaInicio = DateTime.UtcNow.Date.AddDays(10),
            FechaCreacion = DateTime.UtcNow
        };
    }
}
