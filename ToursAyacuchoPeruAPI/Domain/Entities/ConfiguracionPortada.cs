using System;

namespace ToursAyacuchoPeruAPI.Domain.Entities
{
    public class ConfiguracionPortada
    {
        public int ConfiguracionPortadaId { get; set; }
        public string CompanyName { get; set; } = null!;
        public string CompanySubtitle { get; set; } = null!;
        public string? LogoUrl { get; set; }
        public string HeroBadge { get; set; } = null!;
        public string HeroTitle { get; set; } = null!;
        public string HeroSubtitle { get; set; } = null!;
        public string HeroStatsTours { get; set; } = null!;
        public string HeroStatsTravelers { get; set; } = null!;
        public string HeroStatsRating { get; set; } = null!;
        public string HeroImagesJson { get; set; } = null!;
        public DateTime FechaActualizacion { get; set; }
    }
}
