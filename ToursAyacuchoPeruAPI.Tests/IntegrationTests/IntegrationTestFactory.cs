using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ToursAyacuchoPeruAPI.Infrastructure.Persistence;

namespace ToursAyacuchoPeruAPI.Tests.TestSupport;

internal sealed class IntegrationTestFactory : WebApplicationFactory<Program>
{
    private const string TestJwtSecret = "TEST_JWT_SECRET_FOR_INTEGRATION_TESTS_123456789";
    private readonly string _databaseName = $"ToursAyacuchoPeruIntegration-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Secret"] = TestJwtSecret,
                ["JwtSettings:Issuer"] = "ToursAyacuchoPeruAPI",
                ["JwtSettings:Audience"] = "ToursAyacuchoPeruWeb"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ToursAyacuchoPeruDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ToursAyacuchoPeruDbContext>>();
            services.RemoveAll<IHostedService>();

            services.AddDbContext<ToursAyacuchoPeruDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtSecret)),
                    ValidateIssuer = true,
                    ValidIssuer = "ToursAyacuchoPeruAPI",
                    ValidateAudience = true,
                    ValidAudience = "ToursAyacuchoPeruWeb",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = "role"
                };
            });
        });
    }

    public async Task SeedAsync(Func<ToursAyacuchoPeruDbContext, Task> seed)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ToursAyacuchoPeruDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        await seed(db);
    }
}
