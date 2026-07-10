USE ToursAyacuchoPeruDB;
GO

IF COL_LENGTH('dbo.Usuarios', 'FotoUrl') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios
    ADD FotoUrl NVARCHAR(600) NULL;
END;
GO
