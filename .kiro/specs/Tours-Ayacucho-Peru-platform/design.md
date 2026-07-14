# Documento de Diseño Técnico
## Plataforma Web de Gestión de Ventas y Reservas — TOURS AYACUCHO PERÚ

**Proyecto de Investigación Académica**
**Paradigma:** Specification-Driven Development (SSD) con OpenSpec
**Universidad:** Universidad Nacional de San Cristóbal de Huamanga (UNSCH)
**Escuela Profesional:** Ingeniería de Sistemas
**Docente:** Mg. Ing. Richard Zapata Casaverde
**Estudiante:** Anderson Roki Ochoa Medrano
**Año:** 2026

---

## Descripción General (Overview)

La plataforma web de TOURS AYACUCHO PERÚ es un sistema de información de múltiples capas desarrollado bajo el paradigma **Specification-Driven Development (SSD)**, cuya arquitectura garantiza la trazabilidad directa entre cada Spec Delta del documento de requisitos y los componentes técnicos implementados.

El sistema digitaliza los procesos de ventas y reservas de paquetes turísticos de la agencia, eliminando la gestión manual y previniendo condiciones críticas como el overbooking mediante el uso de transacciones ACID sobre Microsoft SQL Server. El MVP obligatorio cubre cuatro módulos: **Autenticación**, **Reservas**, **Pagos** y **Reprogramación**, evaluados bajo la norma **ISO/IEC 25010** (Adecuación Funcional) y la **Escala SUS** (Usabilidad).

### Objetivos de Diseño

- **Corrección transaccional**: garantizar atomicidad en operaciones críticas de reserva y pago mediante propiedades ACID.
- **Seguridad por diseño**: autenticación basada en JWT firmados, hash bcrypt para contraseñas, HTTPS obligatorio.
- **Separación de responsabilidades**: arquitectura de microservicios conceptuales con controladores independientes por dominio.
- **Trazabilidad SSD**: cada componente referencia el Spec Delta que implementa.


---

## Arquitectura del Sistema

### Estilo Arquitectónico

La plataforma adopta una **arquitectura de tres capas** con separación clara entre presentación, lógica de negocio y persistencia, siguiendo el patrón **Layered Architecture** en el backend y una **Single Page Application (SPA)** en el frontend.

```
┌─────────────────────────────────────────────────────────────────┐
│                    CAPA DE PRESENTACIÓN                         │
│              React.js SPA (Single Page Application)            │
│     Auth Pages │ Reservation Pages │ Payment Pages │ Admin      │
└───────────────────────────┬─────────────────────────────────────┘
                            │ HTTPS / REST API (JSON)
┌───────────────────────────▼─────────────────────────────────────┐
│                    CAPA DE APLICACIÓN                           │
│              ASP.NET Core Web API (C#) — RESTful                │
│  ┌──────────────┐ ┌──────────────────┐ ┌───────────────────┐   │
│  │ Auth_Service │ │Reservation_Service│ │  Payment_Service  │   │
│  └──────────────┘ └──────────────────┘ └───────────────────┘   │
│  ┌──────────────────────┐ ┌────────────────────────────────┐    │
│  │ Rescheduling_Service │ │    Notification_Service        │    │
│  └──────────────────────┘ └────────────────────────────────┘    │
│              JWT Middleware │ ACID Transactions                  │
└───────────────────────────┬─────────────────────────────────────┘
                            │ ADO.NET / Entity Framework Core
┌───────────────────────────▼─────────────────────────────────────┐
│                    CAPA DE PERSISTENCIA                         │
│            Microsoft SQL Server (propiedades ACID)              │
│    Usuarios │ Paquetes │ Reservas │ Pagos │ Comprobantes        │
└─────────────────────────────────────────────────────────────────┘
```

### Diagrama de Despliegue

```
┌────────────────────────────────────────────────────────────────┐
│  NAVEGADOR DEL CLIENTE (Chrome / Firefox / Edge >= 110)        │
│  └── React SPA (HTML/CSS/JS compilado)                         │
│       └── Axios HTTP Client → HTTPS → Backend API              │
└────────────────────────────────────────────────────────────────┘
          │ Puerto 443 (HTTPS / TLS 1.2+)
┌─────────▼──────────────────────────────────────────────────────┐
│  SERVIDOR WEB / API (IIS + ASP.NET Core)                       │
│  ├── Controladores REST (/api/v1/*)                            │
│  ├── Middleware: JWT Authentication, CORS, Exception Handler   │
│  ├── Servicios de dominio (Auth, Reservation, Payment, etc.)   │
│  └── Entity Framework Core (ORM) + DbContext                   │
└─────────┬──────────────────────────────────────────────────────┘
          │ Conexión SQL (TCP/IP)
┌─────────▼──────────────────────────────────────────────────────┐
│  SERVIDOR DE BASE DE DATOS (Microsoft SQL Server)              │
│  ├── Base de datos: ToursAyacuchoPeruDB                               │
│  ├── Transacciones ACID con READ COMMITTED SNAPSHOT            │
│  └── Backups automáticos diarios                               │
└────────────────────────────────────────────────────────────────┘
          │ SMTP
┌─────────▼──────────────────────────────────────────────────────┐
│  SERVIDOR DE CORREO (SMTP externo / SendGrid)                  │
│  └── Notification_Service → Correos de bienvenida,            │
│       comprobantes y recordatorios                             │
└────────────────────────────────────────────────────────────────┘
```


---

## Componentes e Interfaces

### Auth_Service (SD-01, SD-02, SD-03, SD-08)

**Responsabilidad:** Gestión del ciclo de vida de cuentas de usuario: registro, autenticación, actualización de perfil y administración de cuentas por el Administrador.

**Interfaces expuestas:**
- `POST /api/v1/auth/register` — Registro de Cliente (SD-01)
- `POST /api/v1/auth/login` — Inicio de sesión y emisión de JWT (SD-02)
- `GET /api/v1/clients/{clientId}/profile` — Consulta de perfil autenticado (SD-03)
- `PUT /api/v1/clients/{clientId}/profile` — Actualización de perfil (SD-03)
- `GET /api/v1/admin/clients` — Listar clientes (SD-08)
- `PATCH /api/v1/admin/clients/{clientId}/status` — Activar/desactivar cuenta (SD-08)

**Dependencias internas:** `Notification_Service` (envío de correo de bienvenida), `JwtService` (generación y validación de tokens).

---

### Reservation_Service (SD-04, SD-07)

**Responsabilidad:** Gestión del ciclo de vida de reservas: creación, consulta y control de disponibilidad con garantías ACID para prevenir overbooking.

**Interfaces expuestas:**
- `POST /api/v1/reservations` — Crear reserva con control de overbooking (SD-04)
- `GET /api/v1/reservations` — Listar reservas del cliente autenticado según el Token_JWT (SD-07)
- `GET /api/v1/reservations/{reservationId}` — Detalle de reserva (SD-07)

**Dependencias internas:** `IDbTransaction` (transacciones ACID en SQL Server).

---

### Payment_Service (SD-05)

**Responsabilidad:** Registro de pagos, validación de montos, actualización atómica del estado de reserva a "CONFIRMADA" y delegación de emisión de comprobante digital.

**Interfaces expuestas:**
- `POST /api/v1/payments` — Registrar pago (SD-05)
- `GET /api/v1/payments/{paymentId}/receipt` — Obtener comprobante digital (SD-05)

**Dependencias internas:** `Reservation_Service` (verificación de estado), `Notification_Service` (envío de comprobante), `IDbTransaction`.

---

### Rescheduling_Service (SD-06)

**Responsabilidad:** Validación de la ventana de reprogramación (≥ 12 horas), verificación de disponibilidad en nueva fecha, y ejecución atómica de liberación y reasignación de asientos.

**Interfaces expuestas:**
- `PATCH /api/v1/reservations/{reservationId}/reschedule` — Reprogramar reserva (SD-06)

**Dependencias internas:** `Reservation_Service` (disponibilidad), `Notification_Service` (confirmación), `IDbTransaction`.

---

### Notification_Service (SD-11)

**Responsabilidad:** Envío de correos electrónicos transaccionales disparados por eventos de los demás servicios. Opera de forma asíncrona con política de reintentos.

**Eventos que procesa:**
- `AccountRegistered` → correo de bienvenida (≤ 60 s)
- `PaymentConfirmed` → comprobante digital (≤ 120 s)
- `ReservationRescheduled` → confirmación de nueva fecha (≤ 60 s)
- `TourReminder24h` → recordatorio automático (job programado)

**Política de reintentos:** hasta 3 intentos con intervalo de 5 minutos ante fallo de envío SMTP.

---

### Package_Service (SD-10)

**Responsabilidad:** CRUD de paquetes turísticos, gestión de disponibilidad de asientos y publicación del catálogo público.

**Interfaces expuestas:**
- `GET /api/v1/packages` — Catálogo público (no requiere autenticación)
- `GET /api/v1/packages/{packageId}` — Detalle público
- `GET /api/v1/admin/packages` — Catálogo completo para administración
- `POST /api/v1/admin/packages` — Crear paquete (Administrador)
- `PUT /api/v1/admin/packages/{packageId}` — Actualizar paquete (Administrador)
- `DELETE /api/v1/admin/packages/{packageId}` — Desactivar paquete (eliminación lógica)

---

### SiteSettings_Service (SD-13)

**Responsabilidad:** Persistir y exponer la identidad visual de la empresa y la configuración de la portada: logo, nombre comercial, textos hero, estadísticas e imagen principal de fondo.

**Interfaces expuestas:**
- `GET /api/v1/site-settings` — Configuración pública de portada (sin autenticación)
- `GET /api/v1/admin/site-settings` — Configuración para el panel administrativo
- `PUT /api/v1/admin/site-settings` — Actualizar portada e identidad visual (Administrador)

**Dependencias internas:** `ConfiguracionPortada` en SQL Server. El frontend usa `localStorage` únicamente como caché/fallback visual cuando la API no responde.


---

## Modelos de Datos

### Diagrama Entidad-Relación (conceptual)

```
┌──────────────┐        ┌───────────────────┐        ┌──────────────────┐
│   Usuarios   │1      *│     Reservas       │*      1│ PaquetesTuristicos│
│──────────────│────────│───────────────────│────────│──────────────────│
│ UsuarioId PK │        │ ReservaId PK       │        │ PaqueteId PK     │
│ Nombre       │        │ UsuarioId FK       │        │ Nombre           │
│ Correo       │        │ PaqueteId FK       │        │ Destino          │
│ HashPassword │        │ FechaInicio        │        │ Descripcion      │
│ Telefono     │        │ CantAsientos       │        │ PrecioUnitario   │
│ Rol          │        │ MontoTotal         │        │ CapacidadTotal   │
│ Estado       │        │ Estado             │        │ AsientosDisp     │
│ FechaRegistro│        │ ContReprogramacion │        │ FechaInicio      │
└──────────────┘        │ FechaCreacion      │        │ FechaFin         │
                        └─────────┬─────────┘        │ Activo           │
                                  │ 1                 └──────────────────┘
                                  │
                                  │ *
                        ┌─────────▼──────────┐
                        │      Pagos          │
                        │────────────────────│
                        │ PagoId PK           │
                        │ ReservaId FK        │
                        │ Monto               │
                        │ MetodoPago          │
                        │ NumReferencia       │
                        │ FechaPago           │
                        │ Estado              │
                        └─────────┬──────────┘
                                  │ 1
                                  │
                                  │ 1
                        ┌─────────▼──────────┐
                        │   Comprobantes      │
                        │────────────────────│
                        │ ComprobanteId PK    │
                        │ PagoId FK           │
                        │ Contenido (JSON)    │
                        │ FechaEmision        │
                        │ EnviadoCorreo       │
                        └────────────────────┘

┌─────────────────────────┐
│  BloqueosCuenta          │
│─────────────────────────│
│ BloqueoId PK             │
│ UsuarioId FK             │
│ IntentosFallidos         │
│ FechaBloqueo             │
│ FechaDesbloqueo          │
└─────────────────────────┘

┌─────────────────────────┐
│  Resenas                 │
│─────────────────────────│
│ ResenaId PK              │
│ UsuarioId FK             │
│ PaqueteId FK             │
│ Calificacion (1-5)       │
│ Comentario               │
│ FechaPublicacion         │
└─────────────────────────┘
```


### Esquema de Base de Datos SQL Server (DDL)

```sql
-- ============================================================
-- TOURS AYACUCHO PERÚ — Schema DDL
-- Microsoft SQL Server
-- ============================================================

CREATE DATABASE ToursAyacuchoPeruDB
    COLLATE Modern_Spanish_CI_AS;
GO

USE ToursAyacuchoPeruDB;
GO

-- Nivel de aislamiento recomendado para control de overbooking
ALTER DATABASE ToursAyacuchoPeruDB
    SET READ_COMMITTED_SNAPSHOT ON;
GO

-- ------------------------------------------------------------
-- Tabla: Usuarios
-- ------------------------------------------------------------
CREATE TABLE Usuarios (
    UsuarioId    UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    Nombre       NVARCHAR(150)     NOT NULL,
    Correo       NVARCHAR(254)     NOT NULL,
    HashPassword NVARCHAR(72)      NOT NULL,  -- bcrypt output (60 chars max + margen)
    Telefono     NVARCHAR(15)      NOT NULL,
    FotoUrl      NVARCHAR(600)     NULL,
    Rol          NVARCHAR(20)      NOT NULL DEFAULT 'Cliente'
                                   CHECK (Rol IN ('Cliente', 'Administrador')),
    Estado       NVARCHAR(20)      NOT NULL DEFAULT 'Activo'
                                   CHECK (Estado IN ('Activo', 'Inactivo', 'Bloqueado')),
    FechaRegistro DATETIME2(0)     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Usuarios PRIMARY KEY (UsuarioId),
    CONSTRAINT UQ_Usuarios_Correo UNIQUE (Correo)
);
GO

-- ------------------------------------------------------------
-- Tabla: BloqueosCuenta
-- ------------------------------------------------------------
CREATE TABLE BloqueosCuenta (
    BloqueoId        INT              NOT NULL IDENTITY(1,1),
    UsuarioId        UNIQUEIDENTIFIER NOT NULL,
    IntentosFallidos TINYINT          NOT NULL DEFAULT 0,
    FechaBloqueo     DATETIME2(0)     NULL,
    FechaDesbloqueo  DATETIME2(0)     NULL,
    CONSTRAINT PK_BloqueosCuenta PRIMARY KEY (BloqueoId),
    CONSTRAINT FK_BloqueosCuenta_Usuarios
        FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId) ON DELETE CASCADE,
    CONSTRAINT UQ_BloqueosCuenta_UsuarioId UNIQUE (UsuarioId)
);
GO

-- ------------------------------------------------------------
-- Tabla: PaquetesTuristicos
-- ------------------------------------------------------------
CREATE TABLE PaquetesTuristicos (
    PaqueteId       UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    Nombre          NVARCHAR(200)    NOT NULL,
    Destino         NVARCHAR(200)    NOT NULL,
    Descripcion     NVARCHAR(2000)   NULL,
    ImagenUrl       NVARCHAR(600)    NULL,
    PrecioUnitario  DECIMAL(10,2)    NOT NULL CHECK (PrecioUnitario > 0),
    CapacidadTotal  INT              NOT NULL CHECK (CapacidadTotal > 0),
    AsientosDisp    INT              NOT NULL CHECK (AsientosDisp >= 0),
    FechaInicio     DATE             NOT NULL,
    FechaFin        DATE             NOT NULL,
    Activo          BIT              NOT NULL DEFAULT 1,
    FechaCreacion   DATETIME2(0)     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_PaquetesTuristicos PRIMARY KEY (PaqueteId),
    CONSTRAINT CK_PaquetesTuristicos_Fechas
        CHECK (FechaFin >= FechaInicio),
    CONSTRAINT CK_PaquetesTuristicos_Asientos
        CHECK (AsientosDisp <= CapacidadTotal)
);
GO

-- ------------------------------------------------------------
-- Tabla: Reservas
-- ------------------------------------------------------------
CREATE TABLE Reservas (
    ReservaId          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    UsuarioId          UNIQUEIDENTIFIER NOT NULL,
    PaqueteId          UNIQUEIDENTIFIER NOT NULL,
    FechaInicio        DATE             NOT NULL,
    CantAsientos       INT              NOT NULL CHECK (CantAsientos >= 1),
    MontoTotal         DECIMAL(10,2)    NOT NULL CHECK (MontoTotal > 0),
    Estado             NVARCHAR(20)     NOT NULL DEFAULT 'PENDIENTE_PAGO'
                                        CHECK (Estado IN (
                                            'PENDIENTE_PAGO',
                                            'CONFIRMADA',
                                            'REPROGRAMADA',
                                            'COMPLETADA',
                                            'CANCELADA'
                                        )),
    ContReprogramacion TINYINT          NOT NULL DEFAULT 0,
    FechaCreacion      DATETIME2(0)     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Reservas PRIMARY KEY (ReservaId),
    CONSTRAINT FK_Reservas_Usuarios
        FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId),
    CONSTRAINT FK_Reservas_Paquetes
        FOREIGN KEY (PaqueteId) REFERENCES PaquetesTuristicos(PaqueteId)
);
GO

-- Índice para consulta de reservas por cliente
CREATE INDEX IX_Reservas_UsuarioId ON Reservas(UsuarioId);
GO

-- RN-04-05: defensa transaccional contra reservas pendientes duplicadas
-- para el mismo Cliente y Paquete_Turistico.
CREATE UNIQUE INDEX UQ_Reservas_PendienteUnicaPorPaquete
    ON Reservas(UsuarioId, PaqueteId)
    WHERE Estado = 'PENDIENTE_PAGO';
GO

-- ------------------------------------------------------------
-- Tabla: Pagos
-- ------------------------------------------------------------
CREATE TABLE Pagos (
    PagoId         UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    ReservaId      UNIQUEIDENTIFIER NOT NULL,
    Monto          DECIMAL(10,2)    NOT NULL CHECK (Monto > 0),
    MetodoPago     NVARCHAR(30)     NOT NULL
                                    CHECK (MetodoPago IN (
                                        'TransferenciaBancaria',
                                        'DepositoCuenta',
                                        'Yape',
                                        'Plin'
                                    )),
    NumReferencia  NVARCHAR(100)    NOT NULL,
    FechaPago      DATETIME2(0)     NOT NULL DEFAULT SYSUTCDATETIME(),
    Estado         NVARCHAR(20)     NOT NULL DEFAULT 'Registrado'
                                    CHECK (Estado IN ('Registrado', 'Verificado', 'Rechazado')),
    CONSTRAINT PK_Pagos PRIMARY KEY (PagoId),
    CONSTRAINT FK_Pagos_Reservas
        FOREIGN KEY (ReservaId) REFERENCES Reservas(ReservaId),
    CONSTRAINT UQ_Pagos_Reserva UNIQUE (ReservaId)  -- una reserva, un pago
);
GO

-- ------------------------------------------------------------
-- Tabla: Comprobantes
-- ------------------------------------------------------------
CREATE TABLE Comprobantes (
    ComprobanteId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    PagoId        UNIQUEIDENTIFIER NOT NULL,
    Contenido     NVARCHAR(MAX)    NOT NULL,  -- JSON con datos del comprobante
    FechaEmision  DATETIME2(0)     NOT NULL DEFAULT SYSUTCDATETIME(),
    EnviadoCorreo BIT              NOT NULL DEFAULT 0,
    CONSTRAINT PK_Comprobantes PRIMARY KEY (ComprobanteId),
    CONSTRAINT FK_Comprobantes_Pagos
        FOREIGN KEY (PagoId) REFERENCES Pagos(PagoId),
    CONSTRAINT UQ_Comprobantes_Pago UNIQUE (PagoId)
);
GO

-- ------------------------------------------------------------
-- Tabla: Resenas
-- ------------------------------------------------------------
CREATE TABLE Resenas (
    ResenaId         UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    UsuarioId        UNIQUEIDENTIFIER NOT NULL,
    PaqueteId        UNIQUEIDENTIFIER NOT NULL,
    Calificacion     TINYINT          NOT NULL CHECK (Calificacion BETWEEN 1 AND 5),
    Comentario       NVARCHAR(1000)   NULL,
    FechaPublicacion DATETIME2(0)     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Resenas PRIMARY KEY (ResenaId),
    CONSTRAINT FK_Resenas_Usuarios
        FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId),
    CONSTRAINT FK_Resenas_Paquetes
        FOREIGN KEY (PaqueteId) REFERENCES PaquetesTuristicos(PaqueteId),
    CONSTRAINT UQ_Resenas_UsuarioPaquete
        UNIQUE (UsuarioId, PaqueteId)  -- máximo una reseña por cliente/paquete
);
GO

-- ------------------------------------------------------------
-- Tabla: NotificacionesCliente
-- ------------------------------------------------------------
CREATE TABLE NotificacionesCliente (
    NotificacionId    UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    EventKey          NVARCHAR(300)    NOT NULL,
    DestinatarioEmail NVARCHAR(254)    NOT NULL,
    Asunto            NVARCHAR(200)    NOT NULL,
    Intentos          INT              NOT NULL DEFAULT 0,
    Entregada         BIT              NOT NULL DEFAULT 0,
    UltimoError       NVARCHAR(1000)   NULL,
    FechaCreacion     DATETIME2(0)     NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaEntrega      DATETIME2(0)     NULL,
    CONSTRAINT PK_NotificacionesCliente PRIMARY KEY (NotificacionId),
    CONSTRAINT UQ_NotificacionesCliente_EventKey UNIQUE (EventKey)
);
GO

-- ------------------------------------------------------------
-- Tabla: ConfiguracionPortada
-- ------------------------------------------------------------
CREATE TABLE ConfiguracionPortada (
    ConfiguracionPortadaId INT            NOT NULL,
    CompanyName            NVARCHAR(80)   NOT NULL,
    CompanySubtitle        NVARCHAR(120)  NOT NULL,
    LogoUrl                NVARCHAR(600)  NULL,
    HeroBadge              NVARCHAR(160)  NOT NULL,
    HeroTitle              NVARCHAR(220)  NOT NULL,
    HeroSubtitle           NVARCHAR(600)  NOT NULL,
    HeroStatsTours         NVARCHAR(20)   NOT NULL,
    HeroStatsTravelers     NVARCHAR(20)   NOT NULL,
    HeroStatsRating        NVARCHAR(20)   NOT NULL,
    HeroImagesJson         NVARCHAR(MAX)  NOT NULL,
    FechaActualizacion     DATETIME2(0)   NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ConfiguracionPortada PRIMARY KEY (ConfiguracionPortadaId),
    CONSTRAINT CK_ConfiguracionPortada_Singleton CHECK (ConfiguracionPortadaId = 1),
    CONSTRAINT CK_ConfiguracionPortada_HeroImagesJson CHECK (ISJSON(HeroImagesJson) = 1)
);
GO
```

**Organización vigente de la base de datos:** el único script de instalación mantenido es `database/ToursAyacuchoPeru.sql`. Contiene las nueve tablas y los datos iniciales, pero no crea ni selecciona una base. Las migraciones de Entity Framework Core se conservan en `ToursAyacuchoPeruAPI/Infrastructure/Persistence/Migrations/`. No existe un usuario administrador predeterminado; la primera cuenta administrativa se obtiene registrando un usuario y actualizando su rol de forma controlada.

---

## Diseño de Bajo Nivel

### Estructura de Controladores ASP.NET Core

#### AuthController — SD-01 y SD-02

```csharp
// Controllers/AuthController.cs
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>SD-01: Registro de Cliente (RF01)</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterResponseDto), 201)]
    [ProducesResponseType(typeof(ErrorResponseDto), 400)]
    [ProducesResponseType(typeof(ErrorResponseDto), 409)]
    [ProducesResponseType(typeof(ValidationErrorDto), 422)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return StatusCode(201, result);
    }

    /// <summary>SD-02: Inicio de Sesión (RF02)</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 401)]
    [ProducesResponseType(typeof(ErrorResponseDto), 429)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Ok(result);
    }
}
```

#### ReservationController — SD-04

```csharp
// Controllers/ReservationController.cs
[ApiController]
[Route("api/v1/reservations")]
[Authorize]
public class ReservationController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    /// <summary>SD-04: Crear reserva con control ACID de overbooking (RF04, RF05)</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReservationResponseDto), 201)]
    [ProducesResponseType(typeof(ErrorResponseDto), 409)]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDto dto)
    {
        var clientId = User.GetClientId();  // Extrae del JWT
        var result = await _reservationService.CreateAsync(clientId, dto);
        return StatusCode(201, result);
    }

    /// <summary>SD-06: Reprogramar reserva (CU N°05)</summary>
    [HttpPatch("{reservationId}/reschedule")]
    [ProducesResponseType(typeof(ReservationResponseDto), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 409)]
    [ProducesResponseType(typeof(ErrorResponseDto), 422)]
    public async Task<IActionResult> Reschedule(
        Guid reservationId,
        [FromBody] RescheduleRequestDto dto)
    {
        var clientId = User.GetClientId();
        var result = await _reservationService.RescheduleAsync(clientId, reservationId, dto);
        return Ok(result);
    }
}
```


### Fragmentos de Código C# Críticos

#### 1. Transacción ACID para prevenir Overbooking (SD-04, RN-04-01 a RN-04-03)

```csharp
// Services/ReservationService.cs
public class ReservationService : IReservationService
{
    private readonly ToursAyacuchoPeruDbContext _db;

    public ReservationService(ToursAyacuchoPeruDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Crea una reserva dentro de una transacción ACID con bloqueo pesimista
    /// para prevenir overbooking ante solicitudes concurrentes.
    /// RN-04-01, RN-04-02, RN-04-03
    /// </summary>
    public async Task<ReservationResponseDto> CreateAsync(
        Guid clientId,
        CreateReservationDto dto)
    {
        // Iniciar transacción con nivel de aislamiento Serializable
        // para garantizar exclusión mutua en la validación de disponibilidad
        await using var transaction = await _db.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            // Bloqueo pesimista: adquiere UPDLOCK sobre la fila del paquete
            var paquete = await _db.PaquetesTuristicos
                .FromSqlRaw(
                    "SELECT * FROM PaquetesTuristicos WITH (UPDLOCK, ROWLOCK) " +
                    "WHERE PaqueteId = {0} AND Activo = 1",
                    dto.PaqueteId)
                .FirstOrDefaultAsync();

            if (paquete is null)
                throw new NotFoundException("Paquete turístico no encontrado o inactivo.");

            // RN-04-02: Validar disponibilidad en tiempo real
            if (paquete.AsientosDisp < dto.CantAsientos)
            {
                throw new ConflictException(
                    $"Asientos insuficientes. Disponibles: {paquete.AsientosDisp}.",
                    HttpStatusCode.Conflict);
            }

            // RN-04-05: Verificar reserva duplicada PENDIENTE_PAGO para el mismo paquete
            bool reservaDuplicada = await _db.Reservas.AnyAsync(r =>
                r.UsuarioId == clientId &&
                r.PaqueteId == dto.PaqueteId &&
                r.Estado == EstadoReserva.PendientePago);

            if (reservaDuplicada)
                throw new ConflictException("Ya posee una reserva pendiente de pago para este paquete.");

            // RN-04-03: Operación atómica: crear reserva + descontar asientos
            var reserva = new Reserva
            {
                ReservaId    = Guid.NewGuid(),
                UsuarioId    = clientId,
                PaqueteId    = dto.PaqueteId,
                FechaInicio  = dto.FechaInicio,
                CantAsientos = dto.CantAsientos,
                MontoTotal   = paquete.PrecioUnitario * dto.CantAsientos,
                Estado       = EstadoReserva.PendientePago
            };

            paquete.AsientosDisp -= dto.CantAsientos;  // Descuento atómico

            _db.Reservas.Add(reserva);
            _db.PaquetesTuristicos.Update(paquete);
            await _db.SaveChangesAsync();  // Ambas operaciones o ninguna

            await transaction.CommitAsync();

            return ReservationResponseDto.From(reserva, paquete);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

#### 2. Autenticación JWT — Generación de Token (SD-02, RN-02-01, RN-02-02)

```csharp
// Services/JwtService.cs
public class JwtService : IJwtService
{
    private readonly JwtSettings _settings;

    public JwtService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    /// <summary>
    /// Genera un Token_JWT firmado con HMAC-SHA256.
    /// Payload incluye: sub (clientId), role y exp (8 horas).
    /// RN-02-01: expiración 8 horas. RN-02-02: clientId y rol en payload.
    /// </summary>
    public string GenerateToken(Guid clientId, string rol)
    {
        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(8);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, clientId.ToString()),
            new Claim(ClaimTypes.Role, rol),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer:   _settings.Issuer,
            audience: _settings.Audience,
            claims:   claims,
            expires:  expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

#### 3. Hash bcrypt para Contraseñas (SD-01, RN-01-03, RN-01-04)

```csharp
// Services/AuthService.cs (fragmento de registro)
public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto dto)
{
    // RN-01-01: Verificar unicidad del correo
    bool correoExiste = await _db.Usuarios
        .AnyAsync(u => u.Correo == dto.Correo.ToLowerInvariant());

    if (correoExiste)
        throw new ConflictException("El correo electrónico ya se encuentra registrado.");

    // RN-01-02: La validación de complejidad se aplica en la capa de DTO
    // con anotaciones de DataAnnotations y FluentValidation

    // RN-01-03 y RN-01-04: Hash bcrypt con factor de costo 12
    // NUNCA se almacena la contraseña original
    string hashPassword = BCrypt.Net.BCrypt.HashPassword(
        dto.Password,
        workFactor: 12  // factor de costo >= 10 (RN-01-03)
    );

    var usuario = new Usuario
    {
        UsuarioId     = Guid.NewGuid(),
        Nombre        = dto.Nombre.Trim(),
        Correo        = dto.Correo.ToLowerInvariant(),
        HashPassword  = hashPassword,  // Almacenamiento seguro
        Telefono      = dto.Telefono,
        Rol           = "Cliente"
    };

    _db.Usuarios.Add(usuario);
    await _db.SaveChangesAsync();

    // RN-01-05: Disparar evento de notificación (asíncrono)
    await _notificationService.SendWelcomeEmailAsync(usuario.Correo, usuario.Nombre);

    return new RegisterResponseDto { ClienteId = usuario.UsuarioId };
}
```

#### 4. Bloqueo de Cuenta tras Intentos Fallidos (SD-02, RN-02-03)

```csharp
// Services/AuthService.cs (fragmento de login)
public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
{
    var usuario = await _db.Usuarios
        .FirstOrDefaultAsync(u => u.Correo == dto.Correo.ToLowerInvariant());

    // RN-02-04: Mensaje genérico que no distingue correo vs. contraseña
    const string mensajeGenerico = "Credenciales incorrectas.";

    if (usuario is null || usuario.Estado == EstadoUsuario.Inactivo)
        throw new UnauthorizedException(mensajeGenerico);

    // Verificar si la cuenta está bloqueada (RN-02-03)
    var bloqueo = await _db.BloqueosCuenta
        .FirstOrDefaultAsync(b => b.UsuarioId == usuario.UsuarioId);

    if (bloqueo?.FechaDesbloqueo > DateTime.UtcNow)
    {
        var minutosRestantes = (int)Math.Ceiling(
            (bloqueo.FechaDesbloqueo.Value - DateTime.UtcNow).TotalMinutes);
        throw new TooManyRequestsException(
            $"Cuenta bloqueada. Intente nuevamente en {minutosRestantes} minuto(s).",
            minutosRestantes);
    }

    bool credencialesValidas = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.HashPassword);

    if (!credencialesValidas)
    {
        // Incrementar contador de intentos fallidos
        await IncrementarIntentosFallidosAsync(usuario.UsuarioId, bloqueo);
        throw new UnauthorizedException(mensajeGenerico);
    }

    // Restablecer intentos fallidos tras login exitoso
    if (bloqueo is not null)
    {
        bloqueo.IntentosFallidos = 0;
        bloqueo.FechaBloqueo     = null;
        bloqueo.FechaDesbloqueo  = null;
        await _db.SaveChangesAsync();
    }

    var token   = _jwtService.GenerateToken(usuario.UsuarioId, usuario.Rol);
    var expira  = DateTime.UtcNow.AddHours(8);

    return new LoginResponseDto
    {
        Token    = token,
        ExpiraEn = expira,
        ClienteId = usuario.UsuarioId,
        Rol      = usuario.Rol
    };
}

private async Task IncrementarIntentosFallidosAsync(
    Guid usuarioId, BloqueosCuenta? bloqueo)
{
    if (bloqueo is null)
    {
        bloqueo = new BloqueosCuenta
            { UsuarioId = usuarioId, IntentosFallidos = 1 };
        _db.BloqueosCuenta.Add(bloqueo);
    }
    else
    {
        bloqueo.IntentosFallidos++;
    }

    // RN-02-03: Bloquear tras 5 intentos consecutivos por 15 minutos
    if (bloqueo.IntentosFallidos >= 5)
    {
        bloqueo.FechaBloqueo    = DateTime.UtcNow;
        bloqueo.FechaDesbloqueo = DateTime.UtcNow.AddMinutes(15);
    }

    await _db.SaveChangesAsync();
}
```


#### 5. Transacción ACID para Registro de Pago (SD-05, RN-05-03)

```csharp
// Services/PaymentService.cs
public async Task<PaymentResponseDto> RegisterPaymentAsync(
    Guid clientId,
    RegisterPaymentDto dto)
{
    await using var transaction = await _db.Database
        .BeginTransactionAsync(IsolationLevel.ReadCommitted);

    try
    {
        var reserva = await _db.Reservas
            .Include(r => r.Paquete)
            .Include(r => r.Usuario)
            .FirstOrDefaultAsync(r =>
                r.ReservaId == dto.ReservaId &&
                r.UsuarioId == clientId);

        if (reserva is null)
            throw new NotFoundException("Reserva no encontrada.");

        // RN-05-06: Una reserva CONFIRMADA no admite segundo pago
        if (reserva.Estado == EstadoReserva.Confirmada)
            throw new ConflictException("Esta reserva ya fue pagada.");

        // RN-05-02: Validar monto con tolerancia de S/ 0.01
        decimal montoEsperado = reserva.MontoTotal;
        if (Math.Abs(dto.Monto - montoEsperado) > 0.01m)
        {
            throw new UnprocessableEntityException(
                $"El monto enviado ({dto.Monto:F2}) no coincide con el esperado " +
                $"({montoEsperado:F2}). Tolerancia: S/ 0.01.");
        }

        // RN-05-03: Registrar pago y confirmar reserva en la misma transacción
        var pago = new Pago
        {
            PagoId       = Guid.NewGuid(),
            ReservaId    = reserva.ReservaId,
            Monto        = dto.Monto,
            MetodoPago   = dto.MetodoPago,
            NumReferencia = dto.NumReferencia
        };

        reserva.Estado = EstadoReserva.Confirmada;  // Cambio atómico

        _db.Pagos.Add(pago);
        _db.Reservas.Update(reserva);
        await _db.SaveChangesAsync();

        await transaction.CommitAsync();

        // RN-05-04: Delegar envío de comprobante (asíncrono, fuera de la TX)
        await _notificationService.SendReceiptAsync(pago.PagoId, reserva, pago);

        return PaymentResponseDto.From(pago);
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

#### 6. Transacción ACID para Reprogramación (SD-06, RN-06-01 a RN-06-04)

```csharp
// Services/ReschedulingService.cs
public async Task<ReservationResponseDto> RescheduleAsync(
    Guid clientId,
    Guid reservaId,
    RescheduleRequestDto dto)
{
    await using var transaction = await _db.Database
        .BeginTransactionAsync(IsolationLevel.Serializable);

    try
    {
        var reserva = await _db.Reservas
            .Include(r => r.Paquete)
            .FirstOrDefaultAsync(r =>
                r.ReservaId == reservaId &&
                r.UsuarioId == clientId &&
                r.Estado == EstadoReserva.Confirmada);

        if (reserva is null)
            throw new NotFoundException("Reserva confirmada no encontrada.");

        // RN-06-05: Límite de una reprogramación por reserva
        if (reserva.ContReprogramacion >= 1)
            throw new UnprocessableEntityException(
                "La reserva ya fue reprogramada. Límite: 1 reprogramación.");

        // RN-06-01: Validar ventana de reprogramación (≥ 12 horas de anticipación)
        var horasRestantes = (reserva.FechaInicio.ToDateTime(TimeOnly.MinValue)
            - DateTime.UtcNow).TotalHours;

        if (horasRestantes < 12)
            throw new UnprocessableEntityException(
                $"La solicitud debe realizarse con al menos 12 horas de anticipación. " +
                $"Horas restantes: {horasRestantes:F1}.");

        // RN-06-02: Nueva fecha debe ser futura
        if (dto.NuevaFecha <= DateOnly.FromDateTime(DateTime.UtcNow))
            throw new UnprocessableEntityException(
                "La nueva fecha debe ser posterior a la fecha actual.");

        // RN-06-03: Verificar disponibilidad en nueva fecha con bloqueo pesimista
        var paquete = await _db.PaquetesTuristicos
            .FromSqlRaw(
                "SELECT * FROM PaquetesTuristicos WITH (UPDLOCK, ROWLOCK) " +
                "WHERE PaqueteId = {0}",
                reserva.PaqueteId)
            .FirstOrDefaultAsync();

        if (paquete!.AsientosDisp < reserva.CantAsientos)
            throw new ConflictException(
                $"La nueva fecha no tiene disponibilidad suficiente. " +
                $"Asientos disponibles: {paquete.AsientosDisp}.");

        // RN-06-04: Liberar asientos en fecha original + asignar en nueva fecha (atómico)
        paquete.AsientosDisp -= reserva.CantAsientos;  // Reservar en nueva fecha
        // (En un modelo simplificado; con fechas múltiples se manejarían filas separadas)

        reserva.FechaInicio          = dto.NuevaFecha;
        reserva.Estado               = EstadoReserva.Reprogramada;
        reserva.ContReprogramacion  += 1;

        _db.PaquetesTuristicos.Update(paquete);
        _db.Reservas.Update(reserva);
        await _db.SaveChangesAsync();

        await transaction.CommitAsync();

        // RN-06-06: Notificar al cliente (fuera de la TX)
        await _notificationService.SendRescheduleConfirmationAsync(
            reserva.Usuario!.Correo, reserva);

        return ReservationResponseDto.From(reserva, paquete);
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```


### Estructura de Componentes React.js

La implementación vigente usa React + Vite con archivos `.jsx`, `AuthContext` para sesión, `apiClient.js` para HTTP y `siteSettings.js` como utilidad de lectura/cache de la configuración de portada.

```
src/
├── pages/
│   ├── Home.jsx                    # Portada pública con 4 paquetes destacados
│   ├── Packages.jsx                # SD-10: catálogo completo para Cliente
│   ├── PackageDetail.jsx           # Detalle y acceso a reserva
│   ├── CreateReservation.jsx       # SD-04: flujo de reserva
│   ├── MyReservations.jsx          # SD-07: mis reservas
│   ├── Payment.jsx                 # SD-05: registro de pago
│   ├── Reschedule.jsx              # SD-06: reprogramación
│   ├── Profile.jsx                 # SD-03: perfil con foto URL
│   ├── Login.jsx                   # SD-02: inicio de sesión
│   ├── Register.jsx                # SD-01: registro
│   └── AdminDashboard.jsx          # Admin: dashboard, paquetes, clientes, reservas, reportes y portada
├── components/
│   └── layout/
│       ├── Header.jsx              # Logo dinámico, navegación y avatar
│       ├── Footer.jsx
│       └── ProtectedRoute.jsx
├── context/
│   └── AuthContext.jsx             # Estado de sesión, rol, foto y datos de usuario
├── hooks/
│   └── useAuth.js                  # Acceso simple al AuthContext
├── api/
│   └── apiClient.js                # Cliente Axios con token JWT
└── utils/
    └── siteSettings.js             # GET/PUT de configuración de portada + caché local
```


---

## Spec Deltas Técnicos

### SD-01: Registro de Cliente (RF01)

**Descripción General:** El Auth_Service permite a un visitante crear una cuenta de Cliente verificada mediante el envío de sus datos personales y credenciales. La contraseña se almacena exclusivamente como hash bcrypt con factor de costo mínimo 12.

**Actores:** Visitante (no autenticado), Auth_Service, Notification_Service.

**Precondiciones:** El visitante no posee una cuenta registrada con el correo electrónico proporcionado.

**Reglas de Negocio Estrictas:**
1. El correo electrónico DEBE tener formato válido (RFC 5322) y DEBE ser único en el sistema.
2. La contraseña DEBE tener mínimo 8 caracteres, al menos una mayúscula, un dígito y un carácter especial.
3. El Auth_Service DEBE almacenar la contraseña con hash bcrypt (factor de costo ≥ 10).
4. El Auth_Service NO DEBE persistir la contraseña en texto plano en ningún medio.
5. El Notification_Service DEBE enviar un correo de bienvenida en un máximo de 60 segundos tras el registro exitoso.

**Endpoint API RESTful:** `POST /api/v1/auth/register`

```json
// Request Body
{
  "nombre":    "Anderson Roki Ochoa Medrano",
  "correo":    "anderson.ochoa@email.com",
  "password":  "SecurePass@2026",
  "telefono":  "987654321"
}

// Response 201 Created
{
  "clienteId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "mensaje":   "Cuenta creada exitosamente. Revise su correo electrónico."
}

// Response 409 Conflict
{
  "error":   "CORREO_DUPLICADO",
  "mensaje": "El correo electrónico ya se encuentra registrado en el sistema."
}

// Response 422 Unprocessable Entity
{
  "error":   "PASSWORD_INVALIDA",
  "mensaje": "La contraseña no cumple los requisitos de complejidad.",
  "detalle": [
    "Debe contener al menos una letra mayúscula.",
    "Debe contener al menos un carácter especial (@, #, $, etc.)."
  ]
}
```

---

### SD-02: Inicio de Sesión (RF02)

**Descripción General:** El Auth_Service verifica las credenciales del Cliente y, ante autenticación exitosa, emite un Token_JWT firmado (HMAC-SHA256) con expiración de 8 horas y payload con clientId y rol.

**Actores:** Cliente, Auth_Service.

**Precondiciones:** El Cliente posee una cuenta registrada y activa en el sistema.

**Reglas de Negocio Estrictas:**
1. El Token_JWT DEBE tener expiración de 8 horas desde su emisión.
2. El Token_JWT DEBE incluir en payload: clientId, rol y fecha de expiración.
3. El Auth_Service DEBE bloquear la cuenta 15 minutos tras 5 intentos fallidos consecutivos.
4. El Auth_Service NO DEBE revelar si el error es por correo incorrecto o contraseña incorrecta.

**Endpoint API RESTful:** `POST /api/v1/auth/login`

```json
// Request Body
{
  "correo":   "anderson.ochoa@email.com",
  "password": "SecurePass@2026"
}

// Response 200 OK
{
  "token":    "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiraEn": "2026-06-15T22:30:00Z",
  "clienteId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "rol":      "Cliente"
}

// Response 401 Unauthorized
{
  "error":   "CREDENCIALES_INVALIDAS",
  "mensaje": "Credenciales incorrectas."
}

// Response 429 Too Many Requests
{
  "error":           "CUENTA_BLOQUEADA",
  "mensaje":         "Cuenta bloqueada temporalmente.",
  "minutosRestantes": 14
}
```

---


### SD-03: Consulta y Actualización de Perfil (RF03)

**Descripción General:** El Auth_Service permite que el Cliente o Administrador autenticado consulte y actualice sus datos de contacto permitidos. El identificador del usuario se obtiene y valida desde el Token_JWT; el correo electrónico no es modificable por este flujo. La foto de perfil se almacena como `FotoUrl` y se refleja en `Header.jsx` y `Profile.jsx`.

**Endpoints API RESTful:**
- `GET /api/v1/clients/{clientId}/profile`
- `PUT /api/v1/clients/{clientId}/profile`

```json
// Response 200 OK
{
  "clienteId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "nombre": "Cliente Actualizado",
  "correo": "cliente@example.com",
  "telefono": "999888777",
  "fotoUrl": "https://example.com/perfil.jpg",
  "rol": "Cliente"
}
```

---

### SD-13: Configuración de Portada (RF18)

**Descripción General:** El SiteSettings_Service centraliza la identidad visual de TOURS AYACUCHO PERU en SQL Server. La portada pública consume `GET /api/v1/site-settings`, muestra una sola imagen principal de fondo y conserva cuatro paquetes destacados como bloque comercial. El panel administrador expone una pestaña "Portada" para editar logo, nombre, textos, estadísticas e imagen principal.

**Endpoints API RESTful:**
- `GET /api/v1/site-settings`
- `GET /api/v1/admin/site-settings`
- `PUT /api/v1/admin/site-settings`

```json
// Response 200 OK
{
  "companyName": "TOURS",
  "companySubtitle": "AYACUCHO PERU",
  "logoUrl": "https://example.com/logo.png",
  "heroBadge": "La Joya de los Andes Peruanos",
  "heroTitle": "Descubre la Magia de Ayacucho Peru",
  "heroSubtitle": "Tours exclusivos, experiencias unicas e inolvidables.",
  "heroStatsTours": "50+",
  "heroStatsTravelers": "1K+",
  "heroStatsRating": "4.9",
  "heroImages": [
    {
      "title": "Cascadas de Campanayuq",
      "url": "https://example.com/hero.jpg"
    }
  ]
}
```

### SD-04: Realizar Reserva (RF04, RF05)

**Descripción General:** El Reservation_Service gestiona la creación de una reserva con validación de disponibilidad en tiempo real y actualización atómica de asientos disponibles mediante transacciones ACID con bloqueo pesimista (UPDLOCK) para prevenir overbooking.

**Actores:** Cliente, Reservation_Service.

**Precondiciones:** El Cliente posee Token_JWT válido. El paquete turístico existe y está activo.

**Reglas de Negocio Estrictas:**
1. El Reservation_Service DEBE validar disponibilidad dentro de una transacción ACID antes de confirmar la reserva.
2. El Reservation_Service NO DEBE confirmar reservas si los asientos solicitados superan la disponibilidad.
3. La reducción de asientos DEBE ejecutarse atómicamente con la creación del registro de reserva.
4. El estado inicial de toda reserva confirmada DEBE ser "PENDIENTE_PAGO".
5. Un Cliente NO DEBE tener más de una reserva con estado "PENDIENTE_PAGO" para el mismo paquete.

**Endpoint API RESTful:** `POST /api/v1/reservations`

```json
// Request Body
{
  "paqueteId":   "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "fechaInicio": "2026-07-15",
  "cantAsientos": 2
}

// Response 201 Created
{
  "reservaId":   "8d3e7c1a-2b4f-4a89-9d0e-1f5c6b7a8e9d",
  "estado":      "PENDIENTE_PAGO",
  "montoTotal":  450.00,
  "paquete": {
    "nombre":  "Tour Huanta – Laguna de Carhuancho",
    "destino": "Huanta, Ayacucho"
  },
  "fechaCreacion": "2026-06-15T14:30:00Z"
}

// Response 409 Conflict
{
  "error":               "ASIENTOS_INSUFICIENTES",
  "mensaje":             "No hay asientos disponibles para la solicitud.",
  "asientosDisponibles": 1
}
```

---

### SD-05: Realizar Pago (RF06, RF07)

**Descripción General:** El Payment_Service registra el pago de una reserva PENDIENTE_PAGO, valida el monto con tolerancia de S/ 0.01, actualiza el estado de la reserva a "CONFIRMADA" dentro de una transacción ACID y delega la emisión del comprobante digital al Notification_Service.

**Actores:** Cliente, Payment_Service, Notification_Service.

**Precondiciones:** El Cliente posee Token_JWT válido. La reserva existe con estado "PENDIENTE_PAGO" y pertenece al Cliente.

**Reglas de Negocio Estrictas:**
1. El Payment_Service DEBE aceptar los métodos: TransferenciaBancaria, DepositoCuenta, Yape y Plin.
2. El monto registrado DEBE coincidir con el monto total calculado, con tolerancia de S/ 0.01.
3. El cambio de estado a "CONFIRMADA" DEBE ejecutarse en la misma transacción del registro de pago.
4. El Notification_Service DEBE enviar el comprobante al correo del Cliente en máximo 120 segundos.
5. El comprobante DEBE contener: reservaId, paquete, cliente, monto, método, referencia y fecha/hora.
6. Una reserva con estado "CONFIRMADA" NO DEBE admitir un segundo registro de pago.

**Endpoint API RESTful:** `POST /api/v1/payments`

```json
// Request Body
{
  "reservaId":     "8d3e7c1a-2b4f-4a89-9d0e-1f5c6b7a8e9d",
  "monto":         450.00,
  "metodoPago":    "Yape",
  "numReferencia": "YPE20260615143022"
}

// Response 201 Created
{
  "pagoId":        "c1d2e3f4-a5b6-7890-c1d2-e3f4a5b67890",
  "estado":        "Registrado",
  "reservaEstado": "CONFIRMADA",
  "mensaje":       "Pago registrado. Comprobante enviado a su correo electrónico."
}

// Response 422 Unprocessable Entity (monto incorrecto)
{
  "error":         "MONTO_INVALIDO",
  "mensaje":       "El monto enviado no coincide con el monto esperado.",
  "montoEsperado": 450.00,
  "montoRecibido": 400.00
}

// Response 409 Conflict (reserva ya pagada)
{
  "error":   "RESERVA_YA_CONFIRMADA",
  "mensaje": "Esta reserva ya fue pagada anteriormente."
}
```

---

### SD-06: Reprogramar Reserva (CU N°05)

**Descripción General:** El Rescheduling_Service permite reprogramar una reserva CONFIRMADA con al menos 12 horas de anticipación a la fecha de inicio original, verificando disponibilidad en la nueva fecha y ejecutando la liberación y reasignación de asientos en una única transacción ACID. El límite es una reprogramación por reserva.

**Actores:** Cliente, Rescheduling_Service, Reservation_Service.

**Precondiciones:** El Cliente posee Token_JWT válido. La reserva existe con estado "CONFIRMADA" y pertenece al Cliente. La solicitud se realiza dentro de la ventana de reprogramación (≥ 12 horas).

**Reglas de Negocio Estrictas:**
1. El Rescheduling_Service DEBE rechazar solicitudes con menos de 12 horas de anticipación.
2. La nueva fecha DEBE ser posterior a la fecha actual del sistema.
3. El Rescheduling_Service DEBE verificar disponibilidad en la nueva fecha antes de ejecutar el cambio.
4. La liberación de asientos en fecha original y asignación en nueva fecha DEBEN ejecutarse en una única transacción ACID.
5. El número máximo de reprogramaciones por reserva DEBE ser 1.
6. El Notification_Service DEBE notificar al Cliente en máximo 60 segundos.

**Nota de modelo:** en el MVP actual cada Paquete_Turístico representa una única salida fechada con disponibilidad propia. Para múltiples fechas por paquete se debe agregar una entidad de salida/disponibilidad por fecha.

**Endpoint API RESTful:** `PATCH /api/v1/reservations/{reservationId}/reschedule`

```json
// Request Body
{
  "nuevaFecha": "2026-08-01"
}

// Response 200 OK
{
  "reservaId":   "8d3e7c1a-2b4f-4a89-9d0e-1f5c6b7a8e9d",
  "estado":      "REPROGRAMADA",
  "nuevaFecha":  "2026-08-01",
  "paquete":     "Tour Huanta – Laguna de Carhuancho",
  "mensaje":     "Reserva reprogramada exitosamente. Notificación enviada a su correo."
}

// Response 422 (fuera de ventana de reprogramación)
{
  "error":                    "FUERA_DE_VENTANA",
  "mensaje":                  "La solicitud debe realizarse con al menos 12 horas de anticipación.",
  "horasRestantesParaTour":   10.5,
  "anticipacionMinimaHoras":  12
}

// Response 409 (nueva fecha sin disponibilidad)
{
  "error":   "NUEVA_FECHA_SIN_DISPONIBILIDAD",
  "mensaje": "La fecha solicitada no tiene asientos disponibles.",
  "fechasAlternativasDisponibles": ["2026-08-05", "2026-08-12"]
}

// Response 422 (límite de reprogramaciones alcanzado)
{
  "error":   "LIMITE_REPROGRAMACIONES",
  "mensaje": "Esta reserva ya fue reprogramada. No se permiten reprogramaciones adicionales."
}
```


---

## Propiedades de Corrección

*Una propiedad es una característica o comportamiento que debe mantenerse verdadero en todas las ejecuciones válidas del sistema; esencialmente, es una declaración formal sobre lo que el sistema debe hacer. Las propiedades sirven como puente entre las especificaciones legibles por humanos y las garantías de corrección verificables por máquina.*

Las siguientes propiedades se derivan del análisis de los criterios de aceptación del documento de requisitos. Son adecuadas para verificación mediante pruebas basadas en propiedades (Property-Based Testing — PBT) dado que el sistema incluye lógica de negocio pura (hash de contraseñas, validación de tokens, transacciones ACID, ventanas de tiempo) donde la variación de entradas revela casos límite significativos y 100 iteraciones encuentran más errores que 2 o 3 casos de ejemplo.

---

### Propiedad 1: Almacenamiento seguro de contraseñas (bcrypt)

*Para cualquier* contraseña de registro válida, el valor almacenado en la base de datos DEBE ser un hash bcrypt que (a) sea verificable con `BCrypt.Verify(password, hash) == true` y (b) sea diferente al valor original de la contraseña.

**Valida: Requisito 1.6 (RN-01-03, RN-01-04)**

---

### Propiedad 2: Validación de contraseñas inválidas

*Para cualquier* cadena de texto que viole al menos una de las reglas de complejidad (longitud < 8, sin mayúscula, sin dígito, sin carácter especial), el Auth_Service DEBE rechazar el registro con código HTTP 422 y la lista de criterios incumplidos.

**Valida: Requisito 1.4 (RN-01-02)**

---

### Propiedad 3: Integridad del payload del Token_JWT

*Para cualquier* inicio de sesión exitoso con credenciales válidas, el Token_JWT emitido DEBE contener en su payload los campos `sub` (clientId), `role` y `exp` (fecha de expiración = hora de emisión + 8 horas), verificables mediante decodificación del token.

**Valida: Requisito 2.1, 2.2 (RN-02-01, RN-02-02)**

---

### Propiedad 4: Bloqueo de cuenta tras intentos fallidos

*Para cualquier* cuenta de Cliente registrada, tras exactamente 5 intentos consecutivos de inicio de sesión con contraseña incorrecta, el Auth_Service DEBE responder con HTTP 429 y un tiempo de bloqueo de 15 minutos, rechazando todos los intentos adicionales hasta que expire el período de bloqueo.

**Valida: Requisito 2.4, 2.5 (RN-02-03)**

---

### Propiedad 5: Prevención de overbooking (invariante ACID)

*Para cualquier* paquete turístico con capacidad N asientos y cualquier conjunto de solicitudes de reserva concurrentes cuya suma total de asientos solicitados exceda N, el Reservation_Service DEBE garantizar que el número total de asientos confirmados nunca supere N. El número de asientos disponibles al finalizar todas las transacciones DEBE ser mayor o igual a cero.

**Valida: Requisito 4.2, 4.3, 4.4 (RN-04-01, RN-04-02, RN-04-03)**

---

### Propiedad 6: Atomicidad de la transacción de reserva

*Para cualquier* operación de creación de reserva, si la transacción es interrumpida (por cualquier causa) luego de crear el registro de Reserva pero antes de descontar los asientos disponibles, el sistema DEBE revertir ambas operaciones: ni el registro de Reserva persiste ni los asientos se modifican.

**Valida: Requisito 4.2 (RN-04-03)**

---

### Propiedad 7: Validación de monto de pago con tolerancia

*Para cualquier* reserva con monto total M, el Payment_Service DEBE:
- Aceptar (HTTP 201) cualquier monto enviado dentro del rango [M − 0.01, M + 0.01].
- Rechazar (HTTP 422) cualquier monto fuera de ese rango, independientemente de la magnitud de la desviación.

**Valida: Requisito 5.4 (RN-05-02)**

---

### Propiedad 8: Completitud del comprobante digital

*Para cualquier* pago registrado exitosamente, el comprobante digital emitido DEBE contener todos los campos requeridos: identificador de reserva, nombre del paquete turístico, nombre del cliente, monto pagado, método de pago, número de referencia y fecha/hora del registro. Ningún campo puede estar ausente o nulo.

**Valida: Requisito 5.3 (RN-05-05)**

---

### Propiedad 9: Ventana de reprogramación (12 horas)

*Para cualquier* reserva confirmada con fecha de inicio F, el Rescheduling_Service DEBE:
- Rechazar (HTTP 422) cualquier solicitud de reprogramación recibida cuando el tiempo restante hasta F sea estrictamente menor a 12 horas.
- Permitir continuar el proceso de reprogramación cuando el tiempo restante sea mayor o igual a 12 horas.

Esta propiedad aplica para cualquier valor de F y cualquier momento de solicitud, verificada sobre un rango amplio de valores de tiempo generados aleatoriamente.

**Valida: Requisito 6.4 (RN-06-01)**

---

### Propiedad 10: Atomicidad de la transacción de reprogramación

*Para cualquier* operación de reprogramación de reserva, la liberación de asientos en la fecha original y la asignación en la nueva fecha DEBEN ejecutarse atómicamente. Si la transacción es interrumpida a mitad, el sistema DEBE revertir ambas operaciones: los asientos de la fecha original se mantienen asignados y los de la nueva fecha no se descuentan.

**Valida: Requisito 6.1, 6.2 (RN-06-04)**


---

## Manejo de Errores

### Jerarquía de Excepciones de Dominio

```csharp
// Exceptions/DomainExceptions.cs
public class ToursAyacuchoPeruException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }

    protected ToursAyacuchoPeruException(string message, int statusCode, string errorCode)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode  = errorCode;
    }
}

public class NotFoundException : ToursAyacuchoPeruException
{
    public NotFoundException(string message)
        : base(message, 404, "RECURSO_NO_ENCONTRADO") { }
}

public class ConflictException : ToursAyacuchoPeruException
{
    public ConflictException(string message)
        : base(message, 409, "CONFLICTO") { }
}

public class UnprocessableEntityException : ToursAyacuchoPeruException
{
    public UnprocessableEntityException(string message)
        : base(message, 422, "ENTIDAD_NO_PROCESABLE") { }
}

public class UnauthorizedException : ToursAyacuchoPeruException
{
    public UnauthorizedException(string message)
        : base(message, 401, "NO_AUTORIZADO") { }
}

public class ForbiddenException : ToursAyacuchoPeruException
{
    public ForbiddenException(string message)
        : base(message, 403, "ACCESO_DENEGADO") { }
}

public class TooManyRequestsException : ToursAyacuchoPeruException
{
    public int MinutosRestantes { get; }
    public TooManyRequestsException(string message, int minutosRestantes)
        : base(message, 429, "DEMASIADAS_SOLICITUDES")
    {
        MinutosRestantes = minutosRestantes;
    }
}
```

### Middleware Global de Manejo de Excepciones

```csharp
// Middleware/GlobalExceptionMiddleware.cs
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ToursAyacuchoPeruException ex)
        {
            _logger.LogWarning(ex, "Excepción de dominio: {ErrorCode}", ex.ErrorCode);
            await WriteErrorResponseAsync(context, ex.StatusCode, ex.ErrorCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado del servidor.");
            await WriteErrorResponseAsync(context, 500, "ERROR_INTERNO",
                "Ocurrió un error inesperado. Contacte al administrador.");
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context, int statusCode, string errorCode, string mensaje)
    {
        context.Response.StatusCode  = statusCode;
        context.Response.ContentType = "application/json";

        var body = JsonSerializer.Serialize(new
        {
            error   = errorCode,
            mensaje = mensaje
        });

        await context.Response.WriteAsync(body);
    }
}
```

### Tabla de Códigos de Error por Spec Delta

| Código HTTP | Código de Error               | Spec Delta | Descripción |
|-------------|-------------------------------|------------|-------------|
| 400         | CAMPO_REQUERIDO               | SD-01      | Campo obligatorio ausente |
| 401         | CREDENCIALES_INVALIDAS        | SD-02      | Credenciales incorrectas (mensaje genérico) |
| 401         | TOKEN_INVALIDO                | Todos      | Token JWT ausente, expirado o malformado |
| 403         | ACCESO_DENEGADO               | SD-08      | Rol insuficiente para la operación |
| 403         | CUENTA_DESACTIVADA            | SD-02      | Cuenta desactivada por administrador |
| 404         | RECURSO_NO_ENCONTRADO         | Todos      | Entidad no existe en el sistema |
| 409         | CORREO_DUPLICADO              | SD-01      | Correo electrónico ya registrado |
| 409         | ASIENTOS_INSUFICIENTES        | SD-04      | Overbooking detectado |
| 409         | RESERVA_YA_CONFIRMADA         | SD-05      | Intento de doble pago |
| 409         | NUEVA_FECHA_SIN_DISPONIBILIDAD| SD-06      | Sin asientos en fecha de reprogramación |
| 422         | PASSWORD_INVALIDA             | SD-01      | Contraseña no cumple complejidad |
| 422         | MONTO_INVALIDO                | SD-05      | Monto de pago no coincide |
| 422         | FUERA_DE_VENTANA              | SD-06      | Reprogramación con < 12 h de anticipación |
| 422         | LIMITE_REPROGRAMACIONES       | SD-06      | Segunda reprogramación solicitada |
| 429         | CUENTA_BLOQUEADA              | SD-02      | Bloqueo por intentos fallidos |
| 500         | ERROR_INTERNO                 | Todos      | Error no controlado del servidor |


---

## Estrategia de Pruebas

### Estado verificado de las pruebas

El proyecto real de pruebas es `ToursAyacuchoPeruAPI.Tests` y utiliza xUnit sobre .NET 10. Al 14 de julio de 2026 contiene pruebas unitarias para autenticación, clientes, reseñas, paquetes, pagos, configuración de portada y validadores, además de pruebas de integración para reservas y reportes administrativos. La última ejecución completó **46 de 46 casos** correctamente.

Las propiedades PBT descritas a continuación forman parte de la estrategia objetivo, pero todavía no están implementadas con FsCheck. También siguen pendientes las pruebas contra SQL Server real, carga, responsividad completa y verificación de producción.

### Enfoque Dual: Pruebas de Ejemplo + Pruebas Basadas en Propiedades

La estrategia de pruebas integra dos enfoques complementarios alineados con el paradigma SSD:

1. **Pruebas de ejemplo (unit tests y pruebas de integración)**: verifican comportamientos específicos con datos concretos, condiciones de error y flujos de integración entre componentes.
2. **Pruebas basadas en propiedades (PBT)**: verifican propiedades universales sobre rangos amplios de entradas generadas aleatoriamente, cubriendo los módulos de autenticación, reservas, pagos y reprogramación.

### Biblioteca PBT Seleccionada

**FsCheck** (para .NET / C#) — biblioteca madura de property-based testing que se integra directamente con xUnit y NUnit. Ejecuta cada propiedad con mínimo 100 iteraciones por defecto y proporciona reducción automática (shrinking) de contraejemplos.

```
NuGet: FsCheck.Xunit (v2.16.x)
```

### Configuración de Pruebas PBT

```csharp
// Tests/Properties/AuthPropertyTests.cs
using FsCheck;
using FsCheck.Xunit;

[Properties(MaxTest = 200)]  // 200 iteraciones para cobertura adicional
public class AuthPropertyTests
{
    // Feature: tours-ayacucho-peru-platform, Propiedad 1: Almacenamiento seguro de contraseñas
    [Property]
    public Property AlmacenamientoSeguroPassword(
        NonEmptyString basePassword)
    {
        // Generar contraseña válida a partir de la base
        var password = EnsureComplexPassword(basePassword.Get);
        var hash     = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

        return (hash != password && BCrypt.Net.BCrypt.Verify(password, hash))
            .ToProperty()
            .Label("El hash debe ser diferente al original y verificable");
    }

    // Feature: tours-ayacucho-peru-platform, Propiedad 2: Validación de contraseñas inválidas
    [Property]
    public Property PasswordInvalidaEsRechazada(
        PositiveInt length)
    {
        // Generar contraseña que viola al menos un criterio
        var invalidPassword = new string('a', Math.Min(length.Get + 1, 7));  // < 8 chars
        var validator       = new PasswordValidator();
        var result          = validator.Validate(invalidPassword);

        return (!result.IsValid).ToProperty()
            .Label("Contraseñas con menos de 8 caracteres deben ser rechazadas");
    }

    private static string EnsureComplexPassword(string base64)
        => $"A1@{base64.Substring(0, Math.Min(base64.Length, 5))}z";
}
```

```csharp
// Tests/Properties/ReservationPropertyTests.cs
[Properties(MaxTest = 150)]
public class ReservationPropertyTests
{
    // Feature: tours-ayacucho-peru-platform, Propiedad 5: Prevención de overbooking
    [Property]
    public async Task<Property> OverbookingPrevenido(
        PositiveInt capacidad,
        PositiveInt solicitudesExtra)
    {
        int n = capacidad.Get % 10 + 1;  // capacidad entre 1 y 10
        int solicitudes = n + solicitudesExtra.Get % 5 + 1;  // más de lo disponible

        var confirmadasCount = await SimularReservasConcurrentesAsync(n, solicitudes);

        return (confirmadasCount <= n).ToProperty()
            .Label($"Máximo {n} reservas deben confirmarse para capacidad {n}");
    }

    // Feature: tours-ayacucho-peru-platform, Propiedad 7: Validación de monto con tolerancia
    [Property]
    public Property MontoConToleranciaAceptado(
        PositiveInt baseAmount,
        NormalFloat delta)
    {
        decimal montoBase   = baseAmount.Get * 10m;
        decimal deltaVal    = (decimal)(delta.Get * 0.005);  // variación pequeña
        decimal montoEnviar = montoBase + deltaVal;

        bool dentroTolerancia = Math.Abs(montoEnviar - montoBase) <= 0.01m;
        bool validacionResult = PaymentValidator.ValidateMonto(montoEnviar, montoBase);

        return (dentroTolerancia == validacionResult).ToProperty()
            .Label("La validación de monto debe coincidir con el criterio de tolerancia");
    }

    // Feature: tours-ayacucho-peru-platform, Propiedad 9: Ventana de reprogramación
    [Property]
    public Property VentanaReprogramacionCorrecta(
        PositiveInt horasAnticipacion)
    {
        double horas = horasAnticipacion.Get % 48;  // entre 0 y 47 horas
        var fechaInicio = DateTime.UtcNow.AddHours(horas);
        bool puedeReprogramar = ReschedulingValidator
            .IsWithinRescheduleWindow(fechaInicio, DateTime.UtcNow);

        return (puedeReprogramar == (horas >= 12)).ToProperty()
            .Label($"Con {horas}h de anticipación, " +
                   $"puedeReprogramar={horas >= 12} debe ser correcto");
    }
}
```

### Estructura General de Pruebas por Módulo

#### Módulo de Autenticación (Auth_Service)

| Tipo | Descripción | Propiedad SSD |
|------|-------------|---------------|
| PBT  | Hash bcrypt verificable para cualquier contraseña | Propiedad 1 |
| PBT  | Cualquier contraseña inválida genera HTTP 422 | Propiedad 2 |
| PBT  | JWT emitido siempre contiene clientId, rol y exp | Propiedad 3 |
| PBT  | Bloqueo tras exactamente 5 intentos fallidos | Propiedad 4 |
| Unit | Registro con correo duplicado genera HTTP 409 | Req 1.3 |
| Unit | Login con correo inexistente genera HTTP 401 genérico | Req 2.6 |
| Integ.| Notification_Service invocado tras registro exitoso | Req 1.2 |

#### Módulo de Reservas (Reservation_Service)

| Tipo | Descripción | Propiedad SSD |
|------|-------------|---------------|
| PBT  | Overbooking imposible bajo solicitudes concurrentes | Propiedad 5 |
| PBT  | Atomicidad: fallo en TX no deja estado inconsistente | Propiedad 6 |
| Unit | Reserva con Token_JWT inválido genera HTTP 401 | Req 4.6 |
| Unit | Consulta retorna sólo reservas del cliente del JWT | Req 7.3 |

#### Módulo de Pagos (Payment_Service)

| Tipo | Descripción | Propiedad SSD |
|------|-------------|---------------|
| PBT  | Montos dentro de tolerancia aceptados; fuera rechazados | Propiedad 7 |
| PBT  | Comprobante siempre contiene todos los campos requeridos | Propiedad 8 |
| Unit | Segunda llamada sobre reserva CONFIRMADA genera HTTP 409 | Req 5.5 |
| Integ.| Notification_Service invocado tras pago exitoso | Req 5.2 |

#### Módulo de Reprogramación (Rescheduling_Service)

| Tipo | Descripción | Propiedad SSD |
|------|-------------|---------------|
| PBT  | Solicitudes con < 12 h siempre rechazadas con HTTP 422 | Propiedad 9 |
| PBT  | Atomicidad de la TX de reprogramación ante fallo | Propiedad 10 |
| Unit | Segunda reprogramación genera HTTP 422 | Req 6.6 |
| Unit | Nueva fecha pasada genera HTTP 422 | Req 6.7 |
| Integ.| Notification_Service invocado tras reprogramación | Req 6.3 |

### Cobertura de Calidad Objetivo

- **Cobertura de líneas:** ≥ 80% en servicios de dominio.
- **Cobertura de ramas:** ≥ 75% en controladores y servicios.
- **Pruebas de integración Postman:** verificación de todos los endpoints de los 5 Spec Deltas del MVP.
- **Evaluación ISO/IEC 25010:** checklist Cumple/No Cumple aplicado a cada criterio de aceptación (meta: 100% de cumplimiento).
- **Evaluación SUS:** puntuación ≥ 80 puntos sobre muestra de 20 sujetos.

---

## Flujo de Datos entre Componentes

```
CLIENTE (React SPA)
        │
        │ POST /api/v1/auth/register
        ▼
AUTH_SERVICE ──── valida formato RFC 5322 ──── USUARIOS (SQL Server)
        │                                           │
        │ hash bcrypt(password, cost=12)            │ INSERT Usuario
        │                                           │
        └──── NOTIFICATION_SERVICE ────────────────┘
                     │
                     │ SMTP (correo bienvenida ≤ 60s)
                     ▼
             SERVIDOR DE CORREO

CLIENTE (React SPA)
        │
        │ POST /api/v1/reservations  [JWT en Header]
        ▼
JWT_MIDDLEWARE ──── valida firma ──── RESERVATION_SERVICE
                                              │
                                    BEGIN TRANSACTION (Serializable)
                                              │
                                    SELECT ... WITH (UPDLOCK)  ──── PAQUETES (SQL Server)
                                              │
                                    validar asientos disponibles
                                              │
                                    INSERT Reserva + UPDATE AsientosDisp
                                              │
                                    COMMIT / ROLLBACK
                                              │
                                    HTTP 201 / HTTP 409
```

---

*Documento de Diseño Técnico generado bajo el paradigma Specification-Driven Development (SSD) con OpenSpec.*
*Versión: 1.0 — Fecha: 2026*
*Alineado con ISO/IEC 25010 (Adecuación Funcional) y Escala SUS (Usabilidad).*
