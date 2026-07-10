USE ToursAyacuchoPeruDB;
GO

IF OBJECT_ID('dbo.ConfiguracionPortada', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConfiguracionPortada (
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
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.ConfiguracionPortada WHERE ConfiguracionPortadaId = 1)
BEGIN
    INSERT INTO dbo.ConfiguracionPortada (
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

