using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ToursAyacuchoPeruAPI.Domain.Entities;
using ToursAyacuchoPeruAPI.Domain.Enums;
using ToursAyacuchoPeruAPI.Tests.TestSupport;

namespace ToursAyacuchoPeruAPI.Tests;

public class AdminReportIntegrationTests : IAsyncLifetime
{
    private readonly IntegrationTestFactory _factory = new();
    private readonly Guid _adminId = Guid.NewGuid();
    private readonly Guid _clientId = Guid.NewGuid();
    private readonly Guid _packageId = Guid.NewGuid();
    private readonly Guid _reservationId = Guid.NewGuid();

    public async Task InitializeAsync()
    {
        await _factory.SeedAsync(async db =>
        {
            db.Usuarios.AddRange(
                CreateUser(_adminId, RolUsuario.Administrador, "admin@tours.test"),
                CreateUser(_clientId, RolUsuario.Cliente, "cliente@tours.test"));

            db.PaquetesTuristicos.Add(new PaqueteTuristico
            {
                PaqueteId = _packageId,
                Nombre = "Aguas Turquesas de Millpu",
                Destino = "Huancaraylla",
                PrecioUnitario = 120m,
                CapacidadTotal = 20,
                AsientosDisp = 18,
                FechaInicio = new DateTime(2026, 7, 20),
                FechaFin = new DateTime(2026, 7, 20),
                Activo = true,
                FechaCreacion = new DateTime(2026, 7, 1)
            });

            db.Reservas.Add(new Reserva
            {
                ReservaId = _reservationId,
                UsuarioId = _clientId,
                PaqueteId = _packageId,
                FechaInicio = new DateTime(2026, 7, 20),
                CantAsientos = 2,
                MontoTotal = 240m,
                Estado = EstadoReserva.CONFIRMADA,
                FechaCreacion = new DateTime(2026, 7, 5, 10, 0, 0)
            });

            db.Pagos.Add(new Pago
            {
                PagoId = Guid.NewGuid(),
                ReservaId = _reservationId,
                Monto = 240m,
                MetodoPago = MetodoPago.Yape,
                NumReferencia = "YAPE-2026-001",
                Estado = EstadoPago.Registrado,
                FechaPago = new DateTime(2026, 7, 5, 10, 15, 0)
            });

            await db.SaveChangesAsync();
        });
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Reservations_report_without_token_returns_401()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/admin/reports/reservations?from=2026-07-01&to=2026-07-31&format=pdf");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Sales_report_with_client_role_returns_403()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CreateJwt(_clientId, RolUsuario.Cliente));

        var response = await client.GetAsync("/api/v1/admin/reports/sales?from=2026-07-01&to=2026-07-31&format=xlsx");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Reservations_report_with_admin_token_returns_pdf_file()
    {
        using var client = CreateAdminClient();

        var response = await client.GetAsync("/api/v1/admin/reports/reservations?from=2026-07-01&to=2026-07-31&format=pdf");
        var content = await response.Content.ReadAsByteArrayAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(content, 0, 4));
        Assert.Contains("reporte-reservas.pdf", response.Content.Headers.ContentDisposition?.FileName);
    }

    [Fact]
    public async Task Sales_report_with_admin_token_returns_xlsx_file()
    {
        using var client = CreateAdminClient();

        var response = await client.GetAsync("/api/v1/admin/reports/sales?from=2026-07-01&to=2026-07-31&format=xlsx");
        var content = await response.Content.ReadAsByteArrayAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal((byte)'P', content[0]);
        Assert.Equal((byte)'K', content[1]);
        Assert.Contains("reporte-ventas.xlsx", response.Content.Headers.ContentDisposition?.FileName);
    }

    [Fact]
    public async Task Report_with_invalid_date_range_returns_422()
    {
        using var client = CreateAdminClient();

        var response = await client.GetAsync("/api/v1/admin/reports/reservations?from=2026-08-01&to=2026-07-31&format=pdf");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains("RANGO_FECHAS_INVALIDO", body);
    }

    private HttpClient CreateAdminClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CreateJwt(_adminId, RolUsuario.Administrador));
        return client;
    }

    private static Usuario CreateUser(Guid userId, RolUsuario role, string email)
    {
        return new Usuario
        {
            UsuarioId = userId,
            Nombre = role == RolUsuario.Administrador ? "Admin Reportes" : "Cliente Reportes",
            Correo = email,
            HashPassword = BCrypt.Net.BCrypt.HashPassword("Correcta@2026"),
            Telefono = "987654321",
            Rol = role,
            Estado = EstadoUsuario.Activo,
            FechaRegistro = new DateTime(2026, 7, 1)
        };
    }

    private static string CreateJwt(Guid userId, RolUsuario role)
    {
        const string secret = "CLAVE_SECRETA_DESARROLLO_TOURS_AYACUCHO_PERU_2026_123456";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("role", role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: "ToursAyacuchoPeruAPI",
            audience: "ToursAyacuchoPeruWeb",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
