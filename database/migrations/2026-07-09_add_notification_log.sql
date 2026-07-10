USE ToursAyacuchoPeruDB;
GO

IF OBJECT_ID(N'dbo.NotificacionesCliente', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NotificacionesCliente (
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
END;
GO

IF OBJECT_ID(N'dbo.NotificacionesCliente', N'U') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.NotificacionesCliente')
          AND name = N'UQ_NotificacionesCliente_EventKey'
   )
BEGIN
    CREATE UNIQUE INDEX UQ_NotificacionesCliente_EventKey
        ON dbo.NotificacionesCliente(EventKey);
END;
GO

