// Tarea 1.3 â€” SD-01 a SD-05: ConfiguraciÃ³n inicial del proyecto â€” TOURS AYACUCHO PERÃš
using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.DataProtection;
using ToursAyacuchoPeruAPI.Infrastructure.Persistence;
using ToursAyacuchoPeruAPI.Presentation.Middleware;
using ToursAyacuchoPeruAPI.Infrastructure.Configuration;
using ToursAyacuchoPeruAPI.Application.Services;
using ToursAyacuchoPeruAPI.Infrastructure.Services;
using ToursAyacuchoPeruAPI.Application.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// En desarrollo las claves de Data Protection se mantienen junto a la API.
// Esto evita reutilizar claves DPAPI del perfil de Windows que pudieron crearse
// con otro usuario/proceso y que impedían responder las solicitudes locales.
if (builder.Environment.IsDevelopment())
{
    var dataProtectionKeysDirectory = new DirectoryInfo(
        Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtection-Keys"));

    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(dataProtectionKeysDirectory);

    // EventLog requiere permisos que una ejecución local normal no siempre tiene.
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();
}

// 1. Configurar Base de Datos con SQL Server
// NOTA: el esquema de la base de datos se crea y mantiene mediante el script
// "database/ToursAyacuchoPeru.sql" (Single Source of Truth del modelo de datos).
// Este proyecto NO usa EF Core Migrations para evitar tener dos fuentes de verdad
// del esquema divergentes entre sÃ­. Ejecute el script .sql directamente en SQL Server
// antes de iniciar la API.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("La cadena de conexiÃ³n 'DefaultConnection' no fue encontrada en la configuraciÃ³n.");

builder.Services.AddDbContext<ToursAyacuchoPeruDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Configurar AutenticaciÃ³n JWT
var jwtSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSection);
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

var secret = jwtSection["Secret"];
if (string.IsNullOrWhiteSpace(secret))
{
    throw new InvalidOperationException("La clave secreta JWT 'Secret' no fue configurada.");
}

// Issuer y Audience no son secretos. Usamos valores estables si el proveedor
// de hosting entrega variables existentes pero vacias.
var issuer = string.IsNullOrWhiteSpace(jwtSection["Issuer"])
    ? "ToursAyacuchoPeruAPI"
    : jwtSection["Issuer"]!;
var audience = string.IsNullOrWhiteSpace(jwtSection["Audience"])
    ? "ToursAyacuchoPeruWeb"
    : jwtSection["Audience"]!;

// JwtService debe emitir el token con los mismos valores efectivos que usa
// JwtBearer para validarlo.
builder.Services.PostConfigure<JwtSettings>(settings =>
{
    settings.Secret = secret;
    settings.Issuer = issuer;
    settings.Audience = audience;
});
var key = Encoding.UTF8.GetBytes(secret);

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminClientService, AdminClientService>();
builder.Services.AddScoped<IPackageService, PackageService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISiteSettingsService, SiteSettingsService>();

// WAVE 5: Servicios de negocio (Transacciones ACID)
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IReschedulingService, ReschedulingService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddHostedService<TourReminderBackgroundService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // RNF02: en producciÃ³n este valor SIEMPRE debe ser 'true' (HTTPS/TLS 1.2+ obligatorio).
    // Se desactiva Ãºnicamente cuando ASPNETCORE_ENVIRONMENT=Development para pruebas locales sin certificado.
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = "role"
    };
});

builder.Services.AddAuthorization();

// 3. Configurar CORS para la integraciÃ³n del Frontend React.
// builder.Configuration combina appsettings y variables de entorno como
// AllowedOrigins__0, AllowedOrigins__1, etc.
var configuredOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>();

var allowedOrigins = configuredOrigins?
    .Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Select(origin => origin.Trim().TrimEnd('/'))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

if (allowedOrigins is null || allowedOrigins.Length == 0)
{
    allowedOrigins =
    [
        "http://localhost:5173",
        "http://localhost:3000"
    ];
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// 4. Configurar Controladores + FluentValidation y OpenAPI/Swagger
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // RF01: campos obligatorios ausentes => 400; datos presentes pero inválidos => 422.
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = new System.Collections.Generic.List<string>();
            var hasMissingRequiredField = false;

            foreach (var kv in context.ModelState)
            {
                if (kv.Value?.ValidationState == ModelValidationState.Invalid)
                {
                    foreach (var err in kv.Value.Errors)
                    {
                        errors.Add(err.ErrorMessage);

                        if (err.ErrorMessage.Contains("field is required", StringComparison.OrdinalIgnoreCase)
                            || err.ErrorMessage.Contains("request body is required", StringComparison.OrdinalIgnoreCase))
                        {
                            hasMissingRequiredField = true;
                        }
                    }
                }
            }

            if (hasMissingRequiredField)
            {
                var badRequestPayload = new
                {
                    error = "CAMPOS_OBLIGATORIOS_AUSENTES",
                    mensaje = "La solicitud no incluye todos los campos obligatorios.",
                    detalle = errors
                };

                return new BadRequestObjectResult(badRequestPayload);
            }

            foreach (var kv in context.ModelState)
            {
                foreach (var err in kv.Value!.Errors)
                {
                    if (!errors.Contains(err.ErrorMessage))
                        errors.Add(err.ErrorMessage);
                }
            }

            var payload = new
            {
                error = "VALIDACION_FALLIDA",
                mensaje = "Los datos enviados no son vÃ¡lidos.",
                detalle = errors
            };
            return new UnprocessableEntityObjectResult(payload);
        };
    });

// Registro automÃ¡tico de todos los validadores FluentValidation del ensamblado
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TOURS AYACUCHO PERÃš API",
        Version = "v1",
        Description = "API del MVP de la plataforma web de gestiÃ³n de ventas y reservas de TOURS AYACUCHO PERÃš Ayacucho 2026."
    });

    // Configurar esquema de seguridad Bearer JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "AutorizaciÃ³n JWT usando la cabecera Authorization con el esquema Bearer. Ejemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// 5. Configurar el pipeline de solicitudes HTTP
// Swagger queda disponible temporalmente en todos los entornos para la presentacion.
// En una operacion publica permanente conviene protegerlo o limitarlo a Development.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TOURS AYACUCHO PERÃš API V1");
});

// Middleware global de excepciones (debe ejecutarse primero)
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
