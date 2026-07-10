using System.Collections.Generic;

namespace ToursAyacuchoPeruAPI.Application.DTOs.SiteSettings
{
    public class HeroImageDto
    {
        public string Title { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
    }

    public class SiteSettingsDto
    {
        public string CompanyName { get; set; } = null!;
        public string CompanySubtitle { get; set; } = null!;
        public string? LogoUrl { get; set; }
        public string HeroBadge { get; set; } = null!;
        public string HeroTitle { get; set; } = null!;
        public string HeroSubtitle { get; set; } = null!;
        public string HeroStatsTours { get; set; } = null!;
        public string HeroStatsTravelers { get; set; } = null!;
        public string HeroStatsRating { get; set; } = null!;
        public List<HeroImageDto> HeroImages { get; set; } = new();
    }
}
