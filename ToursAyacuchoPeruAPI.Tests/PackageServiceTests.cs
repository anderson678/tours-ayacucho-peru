using ToursAyacuchoPeruAPI.Application.DTOs.Packages;
using ToursAyacuchoPeruAPI.Application.Services;
using ToursAyacuchoPeruAPI.Domain.Entities;
using ToursAyacuchoPeruAPI.Domain.Exceptions;
using ToursAyacuchoPeruAPI.Tests.TestSupport;

namespace ToursAyacuchoPeruAPI.Tests;

public class PackageServiceTests
{
    [Fact]
    public async Task CreateAsync_trims_optional_values_and_creates_active_package()
    {
        await using var db = TestDb.Create();
        var service = new PackageService(db);

        var result = await service.CreateAsync(new CreatePackageDto
        {
            Nombre = "  Millpu  ", Destino = "  Huancaraylla  ", Descripcion = "  Aguas turquesas  ",
            ImagenUrl = "  https://example.com/millpu.jpg  ", PrecioUnitario = 120m,
            CapacidadTotal = 20, AsientosDisp = 15, FechaInicio = new DateTime(2027, 1, 10), FechaFin = new DateTime(2027, 1, 11)
        });

        Assert.Equal("Millpu", result.Nombre);
        Assert.Equal("Huancaraylla", result.Destino);
        Assert.True(result.Activo);
        Assert.Equal("Aguas turquesas", result.Descripcion);
        Assert.Single(db.PaquetesTuristicos);
    }

    [Fact]
    public async Task GetActivePackagesAsync_returns_only_active_packages()
    {
        await using var db = TestDb.Create();
        var active = CreatePackage(true, new DateTime(2027, 1, 10));
        db.PaquetesTuristicos.AddRange(active, CreatePackage(false, new DateTime(2027, 1, 5)));
        await db.SaveChangesAsync();

        var result = (await new PackageService(db).GetActivePackagesAsync()).ToList();

        Assert.Single(result);
        Assert.Equal(active.PaqueteId, result[0].PaqueteId);
    }

    [Fact]
    public async Task UpdateAsync_and_DeactivateAsync_persist_changes()
    {
        await using var db = TestDb.Create();
        var package = CreatePackage(true, new DateTime(2027, 1, 10));
        db.PaquetesTuristicos.Add(package);
        await db.SaveChangesAsync();
        var service = new PackageService(db);

        var updated = await service.UpdateAsync(package.PaqueteId, new UpdatePackageDto
        {
            Nombre = "  Wari  ", Destino = "  Quinua  ", Descripcion = " ", ImagenUrl = " ",
            PrecioUnitario = 150m, CapacidadTotal = 30, AsientosDisp = 20,
            FechaInicio = new DateTime(2027, 2, 1), FechaFin = new DateTime(2027, 2, 2), Activo = true
        });
        await service.DeactivateAsync(package.PaqueteId);

        Assert.Equal("Wari", updated.Nombre);
        Assert.Null(updated.Descripcion);
        Assert.False((await db.PaquetesTuristicos.FindAsync(package.PaqueteId))!.Activo);
    }

    [Fact]
    public async Task GetByIdAsync_and_UpdateAsync_throw_when_package_does_not_exist()
    {
        await using var db = TestDb.Create();
        var service = new PackageService(db);
        var id = Guid.NewGuid();

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(id));
        await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateAsync(id, new UpdatePackageDto()));
    }

    private static PaqueteTuristico CreatePackage(bool active, DateTime date) => new()
    {
        PaqueteId = Guid.NewGuid(), Nombre = active ? "Millpu" : "Inactivo", Destino = "Ayacucho",
        PrecioUnitario = 120m, CapacidadTotal = 20, AsientosDisp = 10, FechaInicio = date, FechaFin = date,
        Activo = active, FechaCreacion = DateTime.UtcNow
    };
}
