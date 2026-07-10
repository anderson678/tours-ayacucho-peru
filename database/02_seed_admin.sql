USE ToursAyacuchoPeruDB;
GO

-- Administrador inicial para acceder al panel web.
-- Correo: admin@toursayacuchoperu.com
-- Clave temporal: Admin123@
-- Cambiar esta clave despues de la primera puesta en marcha.
DECLARE @AdminCorreo NVARCHAR(254) = 'admin@toursayacuchoperu.com';
DECLARE @AdminHash NVARCHAR(72) = '$2a$12$MxvH86EVxyIwpC2XDImrkeDWoz6sGUw2XWDId/85jZ9NF16rzJl86';

IF EXISTS (SELECT 1 FROM Usuarios WHERE Correo = @AdminCorreo)
BEGIN
    UPDATE Usuarios
    SET
        Nombre = N'Administrador TOURS AYACUCHO PERU',
        HashPassword = @AdminHash,
        Telefono = '999999999',
        FotoUrl = NULL,
        Rol = 'Administrador',
        Estado = 'Activo'
    WHERE Correo = @AdminCorreo;
END
ELSE
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
        @AdminCorreo,
        @AdminHash,
        '999999999',
        NULL,
        'Administrador',
        'Activo'
    );
END;
GO
