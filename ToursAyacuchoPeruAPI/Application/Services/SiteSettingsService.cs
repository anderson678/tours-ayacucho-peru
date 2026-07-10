using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ToursAyacuchoPeruAPI.Application.DTOs.SiteSettings;
using ToursAyacuchoPeruAPI.Application.Interfaces;
using ToursAyacuchoPeruAPI.Domain.Entities;
using ToursAyacuchoPeruAPI.Domain.Exceptions;
using ToursAyacuchoPeruAPI.Infrastructure.Persistence;

namespace ToursAyacuchoPeruAPI.Application.Services
{
    public class SiteSettingsService : ISiteSettingsService
    {
        private const int SingletonId = 1;
        private readonly ToursAyacuchoPeruDbContext _db;

        public SiteSettingsService(ToursAyacuchoPeruDbContext db)
        {
            _db = db;
        }

        public async Task<SiteSettingsDto> GetAsync()
        {
            var settings = await _db.ConfiguracionPortada
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ConfiguracionPortadaId == SingletonId);

            return settings is null ? GetDefaultSettings() : ToDto(settings);
        }

        public async Task<SiteSettingsDto> UpdateAsync(SiteSettingsDto dto)
        {
            NormalizeAndValidate(dto);

            var settings = await _db.ConfiguracionPortada
                .FirstOrDefaultAsync(s => s.ConfiguracionPortadaId == SingletonId);

            if (settings is null)
            {
                settings = new ConfiguracionPortada { ConfiguracionPortadaId = SingletonId };
                _db.ConfiguracionPortada.Add(settings);
            }

            settings.CompanyName = dto.CompanyName.Trim();
            settings.CompanySubtitle = dto.CompanySubtitle.Trim();
            settings.LogoUrl = string.IsNullOrWhiteSpace(dto.LogoUrl) ? null : dto.LogoUrl.Trim();
            settings.HeroBadge = dto.HeroBadge.Trim();
            settings.HeroTitle = dto.HeroTitle.Trim();
            settings.HeroSubtitle = dto.HeroSubtitle.Trim();
            settings.HeroStatsTours = dto.HeroStatsTours.Trim();
            settings.HeroStatsTravelers = dto.HeroStatsTravelers.Trim();
            settings.HeroStatsRating = dto.HeroStatsRating.Trim();
            settings.HeroImagesJson = JsonSerializer.Serialize(dto.HeroImages);
            settings.FechaActualizacion = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return ToDto(settings);
        }

        private static void NormalizeAndValidate(SiteSettingsDto dto)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.CompanyName)) errors.Add("El nombre corto de la empresa es requerido.");
            if (string.IsNullOrWhiteSpace(dto.CompanySubtitle)) errors.Add("El subtitulo de marca es requerido.");
            if (string.IsNullOrWhiteSpace(dto.HeroBadge)) errors.Add("La etiqueta superior de portada es requerida.");
            if (string.IsNullOrWhiteSpace(dto.HeroTitle)) errors.Add("El titulo principal de portada es requerido.");
            if (string.IsNullOrWhiteSpace(dto.HeroSubtitle)) errors.Add("La descripcion de portada es requerida.");
            if (dto.HeroImages.Count < 1) errors.Add("La portada debe tener al menos una foto principal.");

            foreach (var image in dto.HeroImages.Select((value, index) => new { value, index }))
            {
                if (string.IsNullOrWhiteSpace(image.value.Title))
                    errors.Add($"El nombre de la zona turistica {image.index + 1} es requerido.");
                if (string.IsNullOrWhiteSpace(image.value.ImageUrl))
                    errors.Add($"La foto URL de la zona turistica {image.index + 1} es requerida.");
            }

            if (errors.Count > 0)
                throw new UnprocessableEntityException("La configuracion de portada no es valida.", errors, "PORTADA_INVALIDA");
        }

        private static SiteSettingsDto ToDto(ConfiguracionPortada settings)
        {
            var defaultSettings = GetDefaultSettings();
            var images = defaultSettings.HeroImages;

            try
            {
                images = JsonSerializer.Deserialize<List<HeroImageDto>>(
                    settings.HeroImagesJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? images;
            }
            catch (JsonException)
            {
                images = defaultSettings.HeroImages;
            }

            return new SiteSettingsDto
            {
                CompanyName = settings.CompanyName,
                CompanySubtitle = settings.CompanySubtitle,
                LogoUrl = settings.LogoUrl,
                HeroBadge = settings.HeroBadge,
                HeroTitle = settings.HeroTitle,
                HeroSubtitle = settings.HeroSubtitle,
                HeroStatsTours = settings.HeroStatsTours,
                HeroStatsTravelers = settings.HeroStatsTravelers,
                HeroStatsRating = settings.HeroStatsRating,
                HeroImages = images.Take(4).ToList()
            };
        }

        private static SiteSettingsDto GetDefaultSettings() => new()
        {
            CompanyName = "TOURS",
            CompanySubtitle = "AYACUCHO PERU",
            LogoUrl = null,
            HeroBadge = "La Joya de los Andes Peruanos",
            HeroTitle = "Descubre la Magia de Ayacucho Peru",
            HeroSubtitle = "Sumergete en la riqueza cultural, historica y natural de Huamanga. Tours exclusivos, experiencias unicas e inolvidables.",
            HeroStatsTours = "50+",
            HeroStatsTravelers = "1K+",
            HeroStatsRating = "4.9",
            HeroImages = new List<HeroImageDto>
            {
                new() { Title = "Aguas Turquesas de Millpu", ImageUrl = "https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?auto=format&fit=crop&w=1400&q=80" },
                new() { Title = "Pampa de Ayacucho", ImageUrl = "https://images.unsplash.com/photo-1587595431973-160d0d94add1?auto=format&fit=crop&w=1400&q=80" },
                new() { Title = "Vilcashuaman Inca", ImageUrl = "https://images.unsplash.com/photo-1526392060635-9d6019884377?auto=format&fit=crop&w=1400&q=80" },
                new() { Title = "Huamanga Colonial", ImageUrl = "https://images.unsplash.com/photo-1533105079780-92b9be482077?auto=format&fit=crop&w=1400&q=80" }
            }
        };
    }
}
