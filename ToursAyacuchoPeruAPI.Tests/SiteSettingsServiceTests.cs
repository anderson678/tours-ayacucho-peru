using ToursAyacuchoPeruAPI.Application.DTOs.SiteSettings;
using ToursAyacuchoPeruAPI.Application.Services;
using ToursAyacuchoPeruAPI.Domain.Entities;
using ToursAyacuchoPeruAPI.Domain.Exceptions;
using ToursAyacuchoPeruAPI.Tests.TestSupport;

namespace ToursAyacuchoPeruAPI.Tests;

public class SiteSettingsServiceTests
{
    [Fact]
    public async Task GetAsync_returns_default_settings_when_database_is_empty()
    {
        await using var db = TestDb.Create();
        var service = new SiteSettingsService(db);

        var result = await service.GetAsync();

        Assert.Equal("TOURS", result.CompanyName);
        Assert.Equal("AYACUCHO PERU", result.CompanySubtitle);
        Assert.NotEmpty(result.HeroImages);
    }

    [Fact]
    public async Task UpdateAsync_trims_and_persists_site_settings()
    {
        await using var db = TestDb.Create();
        var service = new SiteSettingsService(db);

        var result = await service.UpdateAsync(new SiteSettingsDto
        {
            CompanyName = " TOURS ",
            CompanySubtitle = " AYACUCHO PERU ",
            LogoUrl = " https://example.com/logo.png ",
            HeroBadge = " Promo ",
            HeroTitle = " Nueva portada ",
            HeroSubtitle = " Texto principal ",
            HeroStatsTours = " 50+ ",
            HeroStatsTravelers = " 1K+ ",
            HeroStatsRating = " 4.9 ",
            HeroImages = new List<HeroImageDto>
            {
                new() { Title = " Millpu ", ImageUrl = " https://example.com/millpu.jpg " }
            }
        });

        var stored = db.ConfiguracionPortada.Single();
        Assert.Equal("TOURS", result.CompanyName);
        Assert.Equal("https://example.com/logo.png", stored.LogoUrl);
        Assert.Contains("Millpu", stored.HeroImagesJson);
    }

    [Fact]
    public async Task UpdateAsync_rejects_settings_without_main_image()
    {
        await using var db = TestDb.Create();
        var service = new SiteSettingsService(db);

        await Assert.ThrowsAsync<UnprocessableEntityException>(() => service.UpdateAsync(new SiteSettingsDto
        {
            CompanyName = "TOURS",
            CompanySubtitle = "AYACUCHO PERU",
            HeroBadge = "Promo",
            HeroTitle = "Titulo",
            HeroSubtitle = "Descripcion",
            HeroStatsTours = "50+",
            HeroStatsTravelers = "1K+",
            HeroStatsRating = "4.9",
            HeroImages = new List<HeroImageDto>()
        }));
    }

    [Fact]
    public async Task GetAsync_limits_hero_images_to_four()
    {
        await using var db = TestDb.Create();
        db.ConfiguracionPortada.Add(new ConfiguracionPortada
        {
            ConfiguracionPortadaId = 1,
            CompanyName = "TOURS",
            CompanySubtitle = "AYACUCHO PERU",
            HeroBadge = "Promo",
            HeroTitle = "Titulo",
            HeroSubtitle = "Descripcion",
            HeroStatsTours = "50+",
            HeroStatsTravelers = "1K+",
            HeroStatsRating = "4.9",
            HeroImagesJson = """
                [
                  {"title":"1","imageUrl":"https://example.com/1.jpg"},
                  {"title":"2","imageUrl":"https://example.com/2.jpg"},
                  {"title":"3","imageUrl":"https://example.com/3.jpg"},
                  {"title":"4","imageUrl":"https://example.com/4.jpg"},
                  {"title":"5","imageUrl":"https://example.com/5.jpg"}
                ]
                """,
            FechaActualizacion = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        var service = new SiteSettingsService(db);

        var result = await service.GetAsync();

        Assert.Equal(4, result.HeroImages.Count);
    }
}
