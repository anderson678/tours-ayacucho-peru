using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToursAyacuchoPeruAPI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncDatabaseWithApiModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('PaquetesTuristicos', 'CapacidadTotal') IS NULL
    ALTER TABLE PaquetesTuristicos ADD CapacidadTotal INT NULL;

IF COL_LENGTH('PaquetesTuristicos', 'AsientosDisp') IS NULL
    ALTER TABLE PaquetesTuristicos ADD AsientosDisp INT NULL;

IF COL_LENGTH('PaquetesTuristicos', 'FechaInicio') IS NULL
    ALTER TABLE PaquetesTuristicos ADD FechaInicio DATE NULL;

IF COL_LENGTH('PaquetesTuristicos', 'FechaFin') IS NULL
    ALTER TABLE PaquetesTuristicos ADD FechaFin DATE NULL;
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('PaquetesTuristicos', 'CuposDisponibles') IS NOT NULL
    EXEC(N'UPDATE PaquetesTuristicos SET CapacidadTotal = COALESCE(CapacidadTotal, CuposDisponibles, 1) WHERE CapacidadTotal IS NULL');
ELSE
    EXEC(N'UPDATE PaquetesTuristicos SET CapacidadTotal = COALESCE(CapacidadTotal, 1) WHERE CapacidadTotal IS NULL');

IF COL_LENGTH('PaquetesTuristicos', 'CuposDisponibles') IS NOT NULL
    EXEC(N'UPDATE PaquetesTuristicos SET AsientosDisp = COALESCE(AsientosDisp, CuposDisponibles, CapacidadTotal, 0) WHERE AsientosDisp IS NULL');
ELSE
    EXEC(N'UPDATE PaquetesTuristicos SET AsientosDisp = COALESCE(AsientosDisp, CapacidadTotal, 0) WHERE AsientosDisp IS NULL');

EXEC(N'UPDATE PaquetesTuristicos SET FechaInicio = COALESCE(FechaInicio, CAST(FechaCreacion AS DATE), CAST(SYSUTCDATETIME() AS DATE)) WHERE FechaInicio IS NULL');

IF COL_LENGTH('PaquetesTuristicos', 'DuracionDias') IS NOT NULL
    EXEC(N'UPDATE PaquetesTuristicos SET FechaFin = COALESCE(FechaFin, DATEADD(DAY, CASE WHEN DuracionDias > 0 THEN DuracionDias - 1 ELSE 0 END, FechaInicio)) WHERE FechaFin IS NULL');
ELSE
    EXEC(N'UPDATE PaquetesTuristicos SET FechaFin = COALESCE(FechaFin, FechaInicio) WHERE FechaFin IS NULL');
");

            migrationBuilder.Sql(@"
ALTER TABLE PaquetesTuristicos ALTER COLUMN CapacidadTotal INT NOT NULL;
ALTER TABLE PaquetesTuristicos ALTER COLUMN AsientosDisp INT NOT NULL;
ALTER TABLE PaquetesTuristicos ALTER COLUMN FechaInicio DATE NOT NULL;
ALTER TABLE PaquetesTuristicos ALTER COLUMN FechaFin DATE NOT NULL;

IF COL_LENGTH('PaquetesTuristicos', 'DuracionDias') IS NOT NULL
    ALTER TABLE PaquetesTuristicos DROP COLUMN DuracionDias;

IF COL_LENGTH('PaquetesTuristicos', 'CuposDisponibles') IS NOT NULL
    ALTER TABLE PaquetesTuristicos DROP COLUMN CuposDisponibles;
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('Reservas', 'FechaInicio') IS NULL
    ALTER TABLE Reservas ADD FechaInicio DATE NULL;

IF COL_LENGTH('Reservas', 'CantAsientos') IS NULL
    ALTER TABLE Reservas ADD CantAsientos INT NULL;
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('Reservas', 'FechaViaje') IS NOT NULL
    EXEC(N'UPDATE Reservas SET FechaInicio = COALESCE(FechaInicio, CAST(FechaViaje AS DATE), CAST(FechaCreacion AS DATE), CAST(SYSUTCDATETIME() AS DATE)) WHERE FechaInicio IS NULL');
ELSE
    EXEC(N'UPDATE Reservas SET FechaInicio = COALESCE(FechaInicio, CAST(FechaCreacion AS DATE), CAST(SYSUTCDATETIME() AS DATE)) WHERE FechaInicio IS NULL');

IF COL_LENGTH('Reservas', 'NumPersonas') IS NOT NULL
    EXEC(N'UPDATE Reservas SET CantAsientos = COALESCE(CantAsientos, NumPersonas, 1) WHERE CantAsientos IS NULL');
ELSE
    EXEC(N'UPDATE Reservas SET CantAsientos = COALESCE(CantAsientos, 1) WHERE CantAsientos IS NULL');
");

            migrationBuilder.Sql(@"
ALTER TABLE Reservas ALTER COLUMN FechaInicio DATE NOT NULL;
ALTER TABLE Reservas ALTER COLUMN CantAsientos INT NOT NULL;

IF EXISTS (SELECT 1 FROM Reservas WHERE ContReprogramacion > 255)
    THROW 51000, 'No se puede convertir Reservas.ContReprogramacion a TINYINT porque existen valores mayores a 255.', 1;

DECLARE @ContReprogramacionDefault sysname;
SELECT @ContReprogramacionDefault = dc.name
FROM sys.default_constraints dc
INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
INNER JOIN sys.tables t ON t.object_id = c.object_id
WHERE t.name = 'Reservas' AND c.name = 'ContReprogramacion';

IF @ContReprogramacionDefault IS NOT NULL
BEGIN
    DECLARE @DropContReprogramacionDefaultSql nvarchar(max);
    SET @DropContReprogramacionDefaultSql = N'ALTER TABLE Reservas DROP CONSTRAINT ' + QUOTENAME(@ContReprogramacionDefault);
    EXEC(@DropContReprogramacionDefaultSql);
END;

ALTER TABLE Reservas ALTER COLUMN ContReprogramacion TINYINT NOT NULL;

IF NOT EXISTS (
    SELECT 1
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
    INNER JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.name = 'Reservas' AND c.name = 'ContReprogramacion')
    ALTER TABLE Reservas ADD CONSTRAINT DF_Reservas_ContReprogramacion DEFAULT 0 FOR ContReprogramacion;

IF COL_LENGTH('Reservas', 'NumPersonas') IS NOT NULL
    ALTER TABLE Reservas DROP COLUMN NumPersonas;

IF COL_LENGTH('Reservas', 'FechaViaje') IS NOT NULL
    ALTER TABLE Reservas DROP COLUMN FechaViaje;
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('Comprobantes', 'EnviadoCorreo') IS NULL
    ALTER TABLE Comprobantes ADD EnviadoCorreo BIT NOT NULL CONSTRAINT DF_Comprobantes_EnviadoCorreo DEFAULT 0;

IF COL_LENGTH('Pagos', 'UrlComprobante') IS NOT NULL
    ALTER TABLE Pagos DROP COLUMN UrlComprobante;
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('BloqueosCuenta') AND name = 'IX_BloqueosCuenta_UsuarioId')
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('BloqueosCuenta') AND name = 'UQ_BloqueosCuenta_UsuarioId')
    EXEC sp_rename N'BloqueosCuenta.IX_BloqueosCuenta_UsuarioId', N'UQ_BloqueosCuenta_UsuarioId', N'INDEX';

IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('Pagos') AND name = 'IX_Pagos_ReservaId')
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('Pagos') AND name = 'UQ_Pagos_Reserva')
    EXEC sp_rename N'Pagos.IX_Pagos_ReservaId', N'UQ_Pagos_Reserva', N'INDEX';

IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('Comprobantes') AND name = 'IX_Comprobantes_PagoId')
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('Comprobantes') AND name = 'UQ_Comprobantes_Pago')
    EXEC sp_rename N'Comprobantes.IX_Comprobantes_PagoId', N'UQ_Comprobantes_Pago', N'INDEX';

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('Reservas') AND name = 'UQ_Reservas_PendienteUnicaPorPaquete')
    CREATE UNIQUE INDEX UQ_Reservas_PendienteUnicaPorPaquete
    ON Reservas (UsuarioId, PaqueteId)
    WHERE Estado = 'PENDIENTE_PAGO';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("/* No reversible downgrade is defined for the legacy database synchronization. */");
        }
    }
}
