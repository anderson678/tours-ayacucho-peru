namespace ToursAyacuchoPeruAPI.Infrastructure.Configuration
{
    public class SmtpSettings
    {
        public string Server { get; set; } = null!;
        public int Port { get; set; } = 587;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string SenderName { get; set; } = "TOURS AYACUCHO PERU";
        public string SenderEmail { get; set; } = null!;
        public bool EnableSsl { get; set; } = true;
    }
}
