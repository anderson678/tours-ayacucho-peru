using ToursAyacuchoPeruAPI.Application.DTOs.Auth;
using ToursAyacuchoPeruAPI.Application.Services;
using ToursAyacuchoPeruAPI.Domain.Entities;
using ToursAyacuchoPeruAPI.Domain.Enums;
using ToursAyacuchoPeruAPI.Domain.Exceptions;
using ToursAyacuchoPeruAPI.Tests.TestSupport;

namespace ToursAyacuchoPeruAPI.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_creates_client_with_normalized_email_and_bcrypt_hash()
    {
        await using var db = TestDb.Create();
        var service = new AuthService(db, new FakeJwtService(), new FakeNotificationService());

        var result = await service.RegisterAsync(new RegisterRequestDto
        {
            Nombre = "  Ana Cliente  ",
            Correo = " ANA@Email.COM ",
            Password = "Seguro@2026",
            Telefono = "987654321"
        });

        var usuario = await db.Usuarios.FindAsync(result.ClienteId);

        Assert.NotNull(usuario);
        Assert.Equal("Ana Cliente", usuario!.Nombre);
        Assert.Equal("ana@email.com", usuario.Correo);
        Assert.Equal(RolUsuario.Cliente, usuario.Rol);
        Assert.NotEqual("Seguro@2026", usuario.HashPassword);
        Assert.True(BCrypt.Net.BCrypt.Verify("Seguro@2026", usuario.HashPassword));
    }

    [Fact]
    public async Task RegisterAsync_rejects_duplicate_email()
    {
        await using var db = TestDb.Create();
        db.Usuarios.Add(CreateUser(correo: "cliente@email.com"));
        await db.SaveChangesAsync();
        var service = new AuthService(db, new FakeJwtService(), new FakeNotificationService());

        await Assert.ThrowsAsync<ConflictException>(() => service.RegisterAsync(new RegisterRequestDto
        {
            Nombre = "Cliente Duplicado",
            Correo = " CLIENTE@email.com ",
            Password = "Seguro@2026",
            Telefono = "987654321"
        }));
    }

    [Fact]
    public async Task LoginAsync_returns_token_and_profile_fields_for_valid_credentials()
    {
        await using var db = TestDb.Create();
        var user = CreateUser(password: "Correcta@2026", fotoUrl: "https://example.com/foto.jpg");
        db.Usuarios.Add(user);
        await db.SaveChangesAsync();
        var jwt = new FakeJwtService();
        var service = new AuthService(db, jwt, new FakeNotificationService());

        var result = await service.LoginAsync(new LoginRequestDto
        {
            Correo = "CLIENTE@EMAIL.COM",
            Password = "Correcta@2026"
        });

        Assert.Equal(user.UsuarioId, result.ClienteId);
        Assert.Equal("Cliente", result.Rol);
        Assert.Equal("Cliente Demo", result.Nombre);
        Assert.Equal("https://example.com/foto.jpg", result.FotoUrl);
        Assert.StartsWith("fake-token-", result.Token);
        Assert.Single(jwt.Calls);
    }

    [Fact]
    public async Task LoginAsync_blocks_account_after_five_failed_attempts()
    {
        await using var db = TestDb.Create();
        var user = CreateUser(password: "Correcta@2026");
        db.Usuarios.Add(user);
        await db.SaveChangesAsync();
        var service = new AuthService(db, new FakeJwtService(), new FakeNotificationService());

        for (var i = 0; i < 4; i++)
        {
            await Assert.ThrowsAsync<UnauthorizedException>(() => service.LoginAsync(new LoginRequestDto
            {
                Correo = user.Correo,
                Password = "Incorrecta@2026"
            }));
        }

        var exception = await Assert.ThrowsAsync<TooManyRequestsException>(() => service.LoginAsync(new LoginRequestDto
        {
            Correo = user.Correo,
            Password = "Incorrecta@2026"
        }));

        var bloqueo = db.BloqueosCuenta.Single(b => b.UsuarioId == user.UsuarioId);
        Assert.Equal(5, bloqueo.IntentosFallidos);
        Assert.True(bloqueo.FechaDesbloqueo > DateTime.UtcNow);
        Assert.Contains("Cuenta bloqueada", exception.Message);
    }

    [Fact]
    public async Task UpdateProfileAsync_updates_allowed_fields_and_clears_empty_photo()
    {
        await using var db = TestDb.Create();
        var user = CreateUser(fotoUrl: "https://example.com/old.jpg");
        db.Usuarios.Add(user);
        await db.SaveChangesAsync();
        var service = new AuthService(db, new FakeJwtService(), new FakeNotificationService());

        var result = await service.UpdateProfileAsync(user.UsuarioId, new UpdateProfileDto
        {
            Nombre = "  Nuevo Nombre  ",
            Telefono = "999888777",
            FotoUrl = " "
        });

        Assert.Equal("Nuevo Nombre", result.Nombre);
        Assert.Equal("999888777", result.Telefono);
        Assert.Null(result.FotoUrl);
        Assert.Equal(user.Correo, result.Correo);
    }

    private static Usuario CreateUser(
        string correo = "cliente@email.com",
        string password = "Correcta@2026",
        string? fotoUrl = null)
    {
        return new Usuario
        {
            UsuarioId = Guid.NewGuid(),
            Nombre = "Cliente Demo",
            Correo = correo,
            HashPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12),
            Telefono = "987654321",
            FotoUrl = fotoUrl,
            Rol = RolUsuario.Cliente,
            Estado = EstadoUsuario.Activo,
            FechaRegistro = DateTime.UtcNow
        };
    }
}
