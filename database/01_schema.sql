-- Tarea 1.1 â€” SD-01 a SD-05: Base de Datos DDL â€” TOURS AYACUCHO PERÃš
-- ============================================================
-- TOURS AYACUCHO PERÃš â€” Schema DDL
-- Microsoft SQL Server
-- ============================================================

CREATE DATABASE ToursAyacuchoPeruDB
    COLLATE Modern_Spanish_CI_AS;
GO

USE ToursAyacuchoPeruDB;
GO

-- Nivel de aislamiento para control de overbooking y alta concurrencia
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
    HashPassword NVARCHAR(72)      NOT NULL,  -- bcrypt output
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
    -- RelaciÃ³n 1-a-1 con Usuarios: cada Usuario tiene a lo sumo un registro de bloqueo.
    -- Sin esta restricciÃ³n, el modelo EF Core (HasOne().WithOne()) y la base de datos
    -- podrÃ­an divergir bajo concurrencia (permitiendo mÃ¡s de una fila por UsuarioId).
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

-- Ãndice para consulta de reservas por cliente (SD-07)
CREATE INDEX IX_Reservas_UsuarioId ON Reservas(UsuarioId);
GO

-- RN-04-05: un Cliente NO DEBE tener mÃ¡s de una Reserva activa con estado
-- 'PENDIENTE_PAGO' para el mismo Paquete_TurÃ­stico. Se valida en ReservationService,
-- pero ademÃ¡s se refuerza aquÃ­ con un Ã­ndice Ãºnico filtrado: esta es la Ãºnica forma
-- de eliminar la condiciÃ³n de carrera entre dos solicitudes concurrentes, ya que la
-- comprobaciÃ³n a nivel de aplicaciÃ³n por sÃ­ sola no es atÃ³mica.
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
    CONSTRAINT UQ_Pagos_Reserva UNIQUE (ReservaId)  -- una reserva, un pago (SD-05)
);
GO

-- ------------------------------------------------------------
-- Tabla: Comprobantes
-- ------------------------------------------------------------
CREATE TABLE Comprobantes (
    ComprobanteId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    PagoId        UNIQUEIDENTIFIER NOT NULL,
    Contenido     NVARCHAR(MAX)    NOT NULL,  -- JSON con datos del comprobante (SD-05)
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
    Calificacion     INT              NOT NULL CHECK (Calificacion BETWEEN 1 AND 5),
    Comentario       NVARCHAR(1000)   NULL,
    FechaPublicacion DATETIME2(0)     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Resenas PRIMARY KEY (ResenaId),
    CONSTRAINT FK_Resenas_Usuarios
        FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId),
    CONSTRAINT FK_Resenas_Paquetes
        FOREIGN KEY (PaqueteId) REFERENCES PaquetesTuristicos(PaqueteId),
    CONSTRAINT UQ_Resenas_UsuarioPaquete
        UNIQUE (UsuarioId, PaqueteId)  -- mÃ¡ximo una reseÃ±a por cliente/paquete (SD-09)
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

IF NOT EXISTS (SELECT 1 FROM ConfiguracionPortada WHERE ConfiguracionPortadaId = 1)
BEGIN
    INSERT INTO ConfiguracionPortada (
        ConfiguracionPortadaId,
        CompanyName,
        CompanySubtitle,
        LogoUrl,
        HeroBadge,
        HeroTitle,
        HeroSubtitle,
        HeroStatsTours,
        HeroStatsTravelers,
        HeroStatsRating,
        HeroImagesJson
    )
    VALUES (
        1,
        N'TOURS',
        N'AYACUCHO PERU',
        NULL,
        N'La Joya de los Andes Peruanos',
        N'Descubre la Magia de Ayacucho Peru',
        N'Sumergete en la riqueza cultural, historica y natural de Huamanga. Tours exclusivos, experiencias unicas e inolvidables.',
        N'50+',
        N'1K+',
        N'4.9',
        N'[
            {"Title":"Aguas Turquesas de Millpu","ImageUrl":"https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?auto=format&fit=crop&w=1400&q=80"},
            {"Title":"Pampa de Ayacucho","ImageUrl":"https://images.unsplash.com/photo-1587595431973-160d0d94add1?auto=format&fit=crop&w=1400&q=80"},
            {"Title":"Vilcashuaman Inca","ImageUrl":"https://images.unsplash.com/photo-1526392060635-9d6019884377?auto=format&fit=crop&w=1400&q=80"},
            {"Title":"Huamanga Colonial","ImageUrl":"https://images.unsplash.com/photo-1533105079780-92b9be482077?auto=format&fit=crop&w=1400&q=80"}
        ]'
    );
END;
GO

-- ------------------------------------------------------------
-- Usuario administrador inicial para operar el panel web.
-- Credenciales temporales:
--   Correo: admin@toursayacuchoperu.com
--   Clave:  Admin123@
-- Cambiar la clave despues de la primera puesta en marcha.
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Correo = 'admin@toursayacuchoperu.com')
BEGIN
    INSERT INTO Usuarios (
        UsuarioId,
        Nombre,
        Correo,
        HashPassword,
        Telefono,
        FotoUrl,
        Rol,
        Estado
    )
    VALUES (
        NEWID(),
        N'Administrador TOURS AYACUCHO PERU',
        'admin@toursayacuchoperu.com',
        '$2a$12$MxvH86EVxyIwpC2XDImrkeDWoz6sGUw2XWDId/85jZ9NF16rzJl86',
        '999999999',
        NULL,
        'Administrador',
        'Activo'
    );
END;
GO

USE ToursAyacuchoPeruDB;
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

