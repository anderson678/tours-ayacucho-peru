// Tarea 2.4 â€” SD-02: ConfiguraciÃ³n JwtSettings â€” TOURS AYACUCHO PERÃš
namespace ToursAyacuchoPeruAPI.Infrastructure.Configuration
{
    public class JwtSettings
    {
        public string Secret { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int ExpiryHours { get; set; } = 8;
    }
}

