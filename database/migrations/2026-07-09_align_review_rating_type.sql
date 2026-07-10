USE ToursAyacuchoPeruDB;
GO

IF COL_LENGTH('dbo.Resenas', 'Calificacion') IS NOT NULL
   AND EXISTS (
        SELECT 1
        FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.Resenas')
          AND name = N'Calificacion'
          AND system_type_id = TYPE_ID(N'tinyint')
   )
BEGIN
    DECLARE @constraintName SYSNAME;

    SELECT TOP (1) @constraintName = cc.name
    FROM sys.check_constraints cc
    INNER JOIN sys.columns c
        ON c.object_id = cc.parent_object_id
       AND cc.definition LIKE N'%Calificacion%'
    WHERE cc.parent_object_id = OBJECT_ID(N'dbo.Resenas')
      AND c.name = N'Calificacion';

    IF @constraintName IS NOT NULL
    BEGIN
        EXEC(N'ALTER TABLE dbo.Resenas DROP CONSTRAINT ' + QUOTENAME(@constraintName));
    END;

    ALTER TABLE dbo.Resenas
    ALTER COLUMN Calificacion INT NOT NULL;

    ALTER TABLE dbo.Resenas
    ADD CONSTRAINT CK_Resenas_Calificacion
        CHECK (Calificacion BETWEEN 1 AND 5);
END;
GO

