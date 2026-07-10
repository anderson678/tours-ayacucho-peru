USE ToursAyacuchoPeruDB;
GO

-- Paquetes turisticos de prueba para visualizar el catalogo.
-- El administrador puede editarlos o desactivarlos desde el panel.
IF NOT EXISTS (SELECT 1 FROM PaquetesTuristicos WHERE Nombre = N'Ruta Wari y Pampa de Ayacucho')
BEGIN
    INSERT INTO PaquetesTuristicos (
        PaqueteId,
        Nombre,
        Destino,
        Descripcion,
        ImagenUrl,
        PrecioUnitario,
        CapacidadTotal,
        AsientosDisp,
        FechaInicio,
        FechaFin,
        Activo
    )
    VALUES
    (
        NEWID(),
        N'Ruta Wari y Pampa de Ayacucho',
        N'Huamanga, Quinua',
        N'Recorrido historico por el Complejo Arqueologico Wari, pueblo de Quinua y Santuario Historico de la Pampa de Ayacucho.',
        N'https://images.unsplash.com/photo-1587595431973-160d0d94add1?auto=format&fit=crop&w=1200&q=80',
        135.00,
        24,
        24,
        '2026-08-10',
        '2026-08-10',
        1
    );
END;

IF NOT EXISTS (SELECT 1 FROM PaquetesTuristicos WHERE Nombre = N'Aguas Turquesas de Millpu')
BEGIN
    INSERT INTO PaquetesTuristicos (
        PaqueteId,
        Nombre,
        Destino,
        Descripcion,
        ImagenUrl,
        PrecioUnitario,
        CapacidadTotal,
        AsientosDisp,
        FechaInicio,
        FechaFin,
        Activo
    )
    VALUES
    (
        NEWID(),
        N'Aguas Turquesas de Millpu',
        N'Huancaraylla, Victor Fajardo',
        N'Full day hacia las piscinas naturales de Millpu, caminata panoramica, paradas fotograficas y asistencia durante todo el recorrido.',
        N'https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?auto=format&fit=crop&w=1200&q=80',
        180.00,
        18,
        18,
        '2026-08-15',
        '2026-08-15',
        1
    );
END;

IF NOT EXISTS (SELECT 1 FROM PaquetesTuristicos WHERE Nombre = N'Vilcashuaman Inca')
BEGIN
    INSERT INTO PaquetesTuristicos (
        PaqueteId,
        Nombre,
        Destino,
        Descripcion,
        ImagenUrl,
        PrecioUnitario,
        CapacidadTotal,
        AsientosDisp,
        FechaInicio,
        FechaFin,
        Activo
    )
    VALUES
    (
        NEWID(),
        N'Vilcashuaman Inca',
        N'Vilcashuaman',
        N'Visita al Templo del Sol y la Luna, Ushnu Inca, Plaza Principal y paradas culturales en ruta.',
        N'https://images.unsplash.com/photo-1526392060635-9d6019884377?auto=format&fit=crop&w=1200&q=80',
        165.00,
        20,
        20,
        '2026-08-22',
        '2026-08-22',
        1
    );
END;

IF NOT EXISTS (SELECT 1 FROM PaquetesTuristicos WHERE Nombre = N'City Tour Huamanga Colonial')
BEGIN
    INSERT INTO PaquetesTuristicos (
        PaqueteId,
        Nombre,
        Destino,
        Descripcion,
        ImagenUrl,
        PrecioUnitario,
        CapacidadTotal,
        AsientosDisp,
        FechaInicio,
        FechaFin,
        Activo
    )
    VALUES
    (
        NEWID(),
        N'City Tour Huamanga Colonial',
        N'Ayacucho Centro',
        N'Tour por la Plaza Mayor, casonas coloniales, iglesias principales, miradores y espacios artesanales de Huamanga.',
        N'https://images.unsplash.com/photo-1533105079780-92b9be482077?auto=format&fit=crop&w=1200&q=80',
        75.00,
        30,
        30,
        '2026-08-05',
        '2026-08-05',
        1
    );
END;

IF NOT EXISTS (SELECT 1 FROM PaquetesTuristicos WHERE Nombre = N'Aventura en Cangallo')
BEGIN
    INSERT INTO PaquetesTuristicos (
        PaqueteId,
        Nombre,
        Destino,
        Descripcion,
        ImagenUrl,
        PrecioUnitario,
        CapacidadTotal,
        AsientosDisp,
        FechaInicio,
        FechaFin,
        Activo
    )
    VALUES
    (
        NEWID(),
        N'Aventura en Cangallo',
        N'Cangallo',
        N'Experiencia de naturaleza con visita a paisajes andinos, aguas termales y paradas de interpretacion local.',
        N'https://images.unsplash.com/photo-1501785888041-af3ef285b470?auto=format&fit=crop&w=1200&q=80',
        150.00,
        16,
        16,
        '2026-09-02',
        '2026-09-02',
        1
    );
END;

IF NOT EXISTS (SELECT 1 FROM PaquetesTuristicos WHERE Nombre = N'Experiencia Ayacucho 3 Dias')
BEGIN
    INSERT INTO PaquetesTuristicos (
        PaqueteId,
        Nombre,
        Destino,
        Descripcion,
        ImagenUrl,
        PrecioUnitario,
        CapacidadTotal,
        AsientosDisp,
        FechaInicio,
        FechaFin,
        Activo
    )
    VALUES
    (
        NEWID(),
        N'Experiencia Ayacucho 3 Dias',
        N'Huamanga, Quinua, Vilcashuaman',
        N'Paquete completo de 3 dias con city tour, Ruta Wari-Quinua y visita cultural a Vilcashuaman.',
        N'https://images.unsplash.com/photo-1464822759023-fed622ff2c3b?auto=format&fit=crop&w=1200&q=80',
        420.00,
        14,
        14,
        '2026-09-10',
        '2026-09-12',
        1
    );
END;
GO
