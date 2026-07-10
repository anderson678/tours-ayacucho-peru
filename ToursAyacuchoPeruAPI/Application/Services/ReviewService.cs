using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ToursAyacuchoPeruAPI.Application.DTOs.Reviews;
using ToursAyacuchoPeruAPI.Application.Interfaces;
using ToursAyacuchoPeruAPI.Domain.Entities;
using ToursAyacuchoPeruAPI.Domain.Enums;
using ToursAyacuchoPeruAPI.Domain.Exceptions;
using ToursAyacuchoPeruAPI.Infrastructure.Persistence;

namespace ToursAyacuchoPeruAPI.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ToursAyacuchoPeruDbContext _db;

        public ReviewService(ToursAyacuchoPeruDbContext db)
        {
            _db = db;
        }

        public async Task<ReviewResponseDto> CreateAsync(Guid clientId, Guid packageId, CreateReviewDto dto)
        {
            var packageExists = await _db.PaquetesTuristicos
                .AsNoTracking()
                .AnyAsync(p => p.PaqueteId == packageId);

            if (!packageExists)
                throw new NotFoundException("Paquete turístico no encontrado.");

            var hasCompletedReservation = await _db.Reservas
                .AsNoTracking()
                .AnyAsync(r => r.UsuarioId == clientId
                    && r.PaqueteId == packageId
                    && r.Estado == EstadoReserva.COMPLETADA);

            if (!hasCompletedReservation)
            {
                throw new ForbiddenException(
                    "Solo puedes publicar una reseña de paquetes turísticos completados.",
                    "RESERVA_COMPLETADA_REQUERIDA");
            }

            var reviewExists = await _db.Resenas
                .AsNoTracking()
                .AnyAsync(r => r.UsuarioId == clientId && r.PaqueteId == packageId);

            if (reviewExists)
                throw new ConflictException("Ya publicaste una reseña para este paquete turístico.", "RESENA_DUPLICADA");

            var review = new Resena
            {
                ResenaId = Guid.NewGuid(),
                UsuarioId = clientId,
                PaqueteId = packageId,
                Calificacion = dto.Calificacion,
                Comentario = string.IsNullOrWhiteSpace(dto.Comentario) ? null : dto.Comentario.Trim(),
                FechaPublicacion = DateTime.UtcNow
            };

            _db.Resenas.Add(review);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                throw new ConflictException("Ya publicaste una reseña para este paquete turístico.", "RESENA_DUPLICADA");
            }

            return new ReviewResponseDto
            {
                ResenaId = review.ResenaId,
                UsuarioId = review.UsuarioId,
                PaqueteId = review.PaqueteId,
                Calificacion = review.Calificacion,
                Comentario = review.Comentario,
                FechaPublicacion = review.FechaPublicacion
            };
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException exception)
        {
            return exception.InnerException is SqlException sqlException
                && (sqlException.Number == 2601 || sqlException.Number == 2627);
        }
    }
}
