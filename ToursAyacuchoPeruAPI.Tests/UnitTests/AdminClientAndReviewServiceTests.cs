using ToursAyacuchoPeruAPI.Application.DTOs.Admin;
using ToursAyacuchoPeruAPI.Application.DTOs.Reviews;
using ToursAyacuchoPeruAPI.Application.Services;
using ToursAyacuchoPeruAPI.Domain.Entities;
using ToursAyacuchoPeruAPI.Domain.Enums;
using ToursAyacuchoPeruAPI.Domain.Exceptions;
using ToursAyacuchoPeruAPI.Tests.TestSupport;

namespace ToursAyacuchoPeruAPI.Tests;

public class AdminClientAndReviewServiceTests
{
    [Fact]
    public async Task AdminClientService_lists_clients_and_updates_allowed_status()
    {
        await using var db = TestDb.Create();
        var client = CreateUser(RolUsuario.Cliente);
        db.Usuarios.AddRange(client, CreateUser(RolUsuario.Administrador));
        await db.SaveChangesAsync();
        var service = new AdminClientService(db);

        var clients = (await service.GetClientsAsync()).ToList();
        var updated = await service.UpdateClientStatusAsync(client.UsuarioId, new UpdateClientStatusDto { Estado = EstadoUsuario.Inactivo });

        Assert.Single(clients);
        Assert.Equal("Inactivo", updated.Estado);
    }

    [Fact]
    public async Task AdminClientService_rejects_invalid_status_and_unknown_client()
    {
        await using var db = TestDb.Create();
        var service = new AdminClientService(db);

        await Assert.ThrowsAsync<UnprocessableEntityException>(() => service.UpdateClientStatusAsync(Guid.NewGuid(), new UpdateClientStatusDto { Estado = EstadoUsuario.Bloqueado }));
        await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateClientStatusAsync(Guid.NewGuid(), new UpdateClientStatusDto { Estado = EstadoUsuario.Activo }));
    }

    [Fact]
    public async Task ReviewService_creates_review_for_completed_reservation()
    {
        await using var db = TestDb.Create();
        var client = CreateUser(RolUsuario.Cliente);
        var package = CreatePackage();
        db.Usuarios.Add(client);
        db.PaquetesTuristicos.Add(package);
        db.Reservas.Add(new Reserva { ReservaId = Guid.NewGuid(), UsuarioId = client.UsuarioId, PaqueteId = package.PaqueteId, CantAsientos = 1, MontoTotal = 120m, Estado = EstadoReserva.COMPLETADA, FechaInicio = DateTime.UtcNow.Date, FechaCreacion = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var result = await new ReviewService(db).CreateAsync(client.UsuarioId, package.PaqueteId, new CreateReviewDto { Calificacion = 5, Comentario = "  Excelente tour  " });

        Assert.Equal(5, result.Calificacion);
        Assert.Equal("Excelente tour", result.Comentario);
        Assert.Single(db.Resenas);
    }

    [Fact]
    public async Task ReviewService_rejects_missing_package_incomplete_reservation_and_duplicate_review()
    {
        await using var db = TestDb.Create();
        var client = CreateUser(RolUsuario.Cliente);
        var package = CreatePackage();
        db.Usuarios.Add(client);
        db.PaquetesTuristicos.Add(package);
        await db.SaveChangesAsync();
        var service = new ReviewService(db);

        await Assert.ThrowsAsync<NotFoundException>(() => service.CreateAsync(client.UsuarioId, Guid.NewGuid(), new CreateReviewDto { Calificacion = 5 }));
        await Assert.ThrowsAsync<ForbiddenException>(() => service.CreateAsync(client.UsuarioId, package.PaqueteId, new CreateReviewDto { Calificacion = 5 }));

        db.Reservas.Add(new Reserva { ReservaId = Guid.NewGuid(), UsuarioId = client.UsuarioId, PaqueteId = package.PaqueteId, CantAsientos = 1, MontoTotal = 120m, Estado = EstadoReserva.COMPLETADA, FechaInicio = DateTime.UtcNow.Date, FechaCreacion = DateTime.UtcNow });
        db.Resenas.Add(new Resena { ResenaId = Guid.NewGuid(), UsuarioId = client.UsuarioId, PaqueteId = package.PaqueteId, Calificacion = 4, FechaPublicacion = DateTime.UtcNow });
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(client.UsuarioId, package.PaqueteId, new CreateReviewDto { Calificacion = 5 }));
    }

    private static Usuario CreateUser(RolUsuario role) => new()
    {
        UsuarioId = Guid.NewGuid(), Nombre = "Usuario", Correo = $"{Guid.NewGuid():N}@test.com", HashPassword = "hash", Telefono = "987654321",
        Rol = role, Estado = EstadoUsuario.Activo, FechaRegistro = DateTime.UtcNow
    };

    private static PaqueteTuristico CreatePackage() => new()
    {
        PaqueteId = Guid.NewGuid(), Nombre = "Millpu", Destino = "Ayacucho", PrecioUnitario = 120m, CapacidadTotal = 20,
        AsientosDisp = 10, FechaInicio = DateTime.UtcNow.Date, FechaFin = DateTime.UtcNow.Date, Activo = true, FechaCreacion = DateTime.UtcNow
    };
}
