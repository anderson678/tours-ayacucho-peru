# TOURS AYACUCHO PERÃš â€” API (versiÃ³n corregida)

Backend ASP.NET Core (.NET 10) para la plataforma de gestiÃ³n de ventas y reservas de
TOURS AYACUCHO PERÃš. Este documento resume quÃ© se corrigiÃ³ respecto a la versiÃ³n anterior
y cÃ³mo poner el proyecto en marcha desde cero.

## 1. Resumen de lo corregido

| # | Problema detectado | CorrecciÃ³n aplicada |
|---|---|---|
| 1 | El cÃ³digo mezclaba dos namespaces incompatibles (`ToursAyacuchoPeruAPI.*` y `DmGoTravel.API.*`, restos de otro proyecto llamado "DM GO TRAVEL") y no compilaba. | Todo el cÃ³digo fue reescrito bajo un Ãºnico namespace raÃ­z `ToursAyacuchoPeruAPI`. |
| 2 | El `DbContext` inyectado en los servicios (`DmGoTravelDbContext`) no existÃ­a en ningÃºn archivo. | Unificado a `ToursAyacuchoPeruDbContext`, definido una sola vez en `Data/`. |
| 3 | Las entidades C# no coincidÃ­an con las columnas del DDL (`CuposDisponibles` vs `AsientosDisp`, `NumPersonas` vs `CantAsientos`, `FechaViaje` vs `FechaInicio`, faltaban `CapacidadTotal`/`FechaInicio`/`FechaFin` en `PaqueteTuristico`). | Entidades y DTOs renombrados para coincidir 1:1 con `database/ToursAyacuchoPeru.sql`, que es la Single Source of Truth del modelo de datos. |
| 4 | Los enums `EstadoReserva`, `EstadoPago` y `MetodoPago` no coincidÃ­an con los valores permitidos por los `CHECK` del DDL (p. ej. `Efectivo` no es un mÃ©todo de pago vÃ¡lido; el estado `PENDIENTE_PAGO` se guardaba como `Pendiente`). | Los nombres de los enums ahora son idÃ©nticos a los valores del DDL, evitando violaciones de `CHECK` en tiempo de ejecuciÃ³n. |
| 5 | `PaymentService` confirmaba el pago pero nunca creaba el `Comprobante` ni llamaba a `INotificationService` (RN-05-04/05-05). `GET /payments/{id}/receipt` siempre devolvÃ­a vacÃ­o. | `PaymentService` ahora genera el `Comprobante` (JSON) dentro de la misma transacciÃ³n y dispara el envÃ­o vÃ­a `INotificationService.SendPaymentReceiptAsync`. |
| 6 | `ReschedulingService` no notificaba al Cliente tras la reprogramaciÃ³n (RN-06-06). | Se agregÃ³ `INotificationService.SendRescheduleConfirmationAsync`, invocado tras el `commit`. |
| 7 | RN-04-05 (una reserva `PENDIENTE_PAGO` por cliente/paquete) solo se validaba en cÃ³digo de aplicaciÃ³n, vulnerable a condiciÃ³n de carrera. | Se agregÃ³ un Ã­ndice Ãºnico filtrado en la base de datos (`UQ_Reservas_PendienteUnicaPorPaquete`) como defensa adicional. |
| 8 | `BloqueosCuenta` no tenÃ­a `UNIQUE(UsuarioId)` en el DDL pese a que el modelo EF asume una relaciÃ³n 1-a-1. | Se agregÃ³ `CONSTRAINT UQ_BloqueosCuenta_UsuarioId UNIQUE (UsuarioId)`. |
| 9 | Secretos (JWT `Secret`, cadena de conexiÃ³n, credenciales SMTP) en texto plano en `appsettings.json`, listos para subirse a git. | `appsettings.json` ahora solo tiene placeholders; los valores reales van en `dotnet user-secrets` (ver secciÃ³n 3). |
| 10 | CORS totalmente abierto (`AllowAnyOrigin`). | CORS ahora restringido a una lista configurable `AllowedOrigins` en `appsettings*.json`. |
| 11 | ExistÃ­an `Migrations/` de EF Core generadas contra el modelo antiguo (con nombres `CuposDisponibles`, etc.), una segunda fuente de verdad del esquema divergente del script SQL. | Se eliminÃ³ la carpeta `Migrations/`. El proyecto **no** usa EF Core Migrations: el esquema se crea una sola vez ejecutando `database/ToursAyacuchoPeru.sql` directamente en SQL Server (ver secciÃ³n 2). AsÃ­ se evita tener dos definiciones del esquema que puedan desincronizarse. |
| 12 | Archivos de plantilla (`WeatherForecastController`, `WeatherForecast.cs`) sin relaciÃ³n con el dominio. | Eliminados. |

> **Nota sobre `.kiro/specs/design.md`:** el documento de diseÃ±o de tu tesis todavÃ­a muestra el
> DDL sin los dos ajustes del punto 7 y 8. Te recomiendo actualizar esos fragmentos en
> `design.md` para que el documento de especificaciÃ³n siga siendo coherente con
> `database/ToursAyacuchoPeru.sql` (que es el archivo que de verdad se ejecuta).

## 2. Puesta en marcha

### 2.1. Base de datos
1. Abre SQL Server Management Studio (o `sqlcmd`).
2. Ejecuta el script completo `database/ToursAyacuchoPeru.sql` (crea la base de datos `ToursAyacuchoPeruDB` y las tablas con sus constraints).
3. No se requiere `dotnet ef database update`: este proyecto no usa Migrations.

El script crea un administrador inicial para acceder al panel web:

- Correo: `admin@toursayacuchoperu.com`
- Clave temporal: `Admin123@`

Cambia esta clave despues de la primera puesta en marcha.

### 2.2. ConfiguraciÃ³n de secretos

**Nunca** completes `ConnectionStrings`, `JwtSettings.Secret` ni `SmtpSettings` directamente en
`appsettings.json` si vas a subir el repositorio a GitHub. Usa `dotnet user-secrets`:

```bash
cd ToursAyacuchoPeruAPI
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost\SQLEXPRESS;Database=ToursAyacuchoPeruDB;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet user-secrets set "JwtSettings:Secret" "un-secreto-aleatorio-de-al-menos-32-caracteres"
dotnet user-secrets set "SmtpSettings:Username" "tu-correo@gmail.com"
dotnet user-secrets set "SmtpSettings:Password" "tu-contraseÃ±a-de-aplicacion"
```

En un ambiente de despliegue real (Azure, IIS, Docker), reemplaza `user-secrets` por variables
de entorno o un gestor de secretos (Azure Key Vault, etc.).

### 2.3. Ejecutar la API

```bash
dotnet restore
dotnet run
```

La API quedarÃ¡ disponible en `http://localhost:5150`, con Swagger UI en `/swagger` (solo en
entorno `Development`).

## 3. Estructura del proyecto

```
ToursAyacuchoPeruAPI/
â”œâ”€â”€ Configuration/       # JwtSettings
â”œâ”€â”€ Controllers/         # AuthController, ClientProfileController, PackageController,
â”‚                        # ReservationController, PaymentController
â”œâ”€â”€ Data/                # ToursAyacuchoPeruDbContext (Fluent API 1:1 con el DDL)
â”œâ”€â”€ DTOs/Auth/           # DTOs de registro/login/perfil
â”œâ”€â”€ Exceptions/          # ToursAyacuchoPeruException y subclases (404/409/401/403/422/429)
â”œâ”€â”€ Extensions/          # ClaimsPrincipalExtensions (User.GetClientId())
â”œâ”€â”€ Middleware/          # GlobalExceptionMiddleware
â”œâ”€â”€ Models/Entities/     # Usuario, BloqueosCuenta, PaqueteTuristico, Reserva, Pago,
â”‚                        # Comprobante, Resena â€” nombres idÃ©nticos al DDL
â”œâ”€â”€ Models/Enums/        # RolUsuario, EstadoUsuario, EstadoReserva, EstadoPago, MetodoPago
â”œâ”€â”€ Services/            # AuthService, ReservationService, PaymentService,
â”‚                        # ReschedulingService, JwtService, NotificationService
â”œâ”€â”€ Services/Dto/        # DTOs de reserva/pago
â”œâ”€â”€ Services/Interfaces/ # Contratos de todos los servicios
â””â”€â”€ Validators/          # FluentValidation (registro, login, perfil)
```

## 4. Siguientes pasos sugeridos

1. Implementar los mÃ³dulos restantes de `tasks.md` (SD-08 gestiÃ³n de cuentas por
   administrador, SD-09 reseÃ±as, SD-10 catÃ¡logo administrable, SD-11 notificaciones reales
   por SMTP en lugar del stub de log).
2. Reemplazar `NotificationService` (actualmente un stub que solo escribe en el log) por una
   integraciÃ³n SMTP real usando `SmtpSettings`.
3. Escribir pruebas unitarias/de integraciÃ³n para `ReservationService`, `PaymentService` y
   `ReschedulingService` (especialmente los escenarios de concurrencia con `UPDLOCK`), como
   indica tu curso de Pruebas y Aseguramiento de la Calidad de Software.
4. Configurar HTTPS con certificado real y `RequireHttpsMetadata = true` antes de cualquier
   despliegue fuera de tu mÃ¡quina de desarrollo.
5. Conectar el frontend React y ajustar `AllowedOrigins` en `appsettings.json` al dominio real.

