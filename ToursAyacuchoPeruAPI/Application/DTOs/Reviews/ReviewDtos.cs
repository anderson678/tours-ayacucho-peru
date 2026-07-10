using System;

namespace ToursAyacuchoPeruAPI.Application.DTOs.Reviews
{
    public class CreateReviewDto
    {
        public int Calificacion { get; set; }
        public string? Comentario { get; set; }
    }

    public class ReviewResponseDto
    {
        public Guid ResenaId { get; set; }
        public Guid UsuarioId { get; set; }
        public Guid PaqueteId { get; set; }
        public int Calificacion { get; set; }
        public string? Comentario { get; set; }
        public DateTime FechaPublicacion { get; set; }
    }
}
