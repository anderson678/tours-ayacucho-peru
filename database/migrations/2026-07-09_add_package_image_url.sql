USE ToursAyacuchoPeruDB;
GO

IF COL_LENGTH('dbo.PaquetesTuristicos', 'ImagenUrl') IS NULL
BEGIN
    ALTER TABLE dbo.PaquetesTuristicos
    ADD ImagenUrl NVARCHAR(600) NULL;
END;
GO

