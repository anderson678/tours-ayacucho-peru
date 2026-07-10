using FluentValidation;
using ToursAyacuchoPeruAPI.Application.DTOs.Reviews;

namespace ToursAyacuchoPeruAPI.Application.Validators
{
    public class CreateReviewValidator : AbstractValidator<CreateReviewDto>
    {
        public CreateReviewValidator()
        {
            RuleFor(x => x.Calificacion)
                .InclusiveBetween(1, 5)
                .WithMessage("La calificación debe ser un valor entero entre 1 y 5.");

            RuleFor(x => x.Comentario)
                .MaximumLength(1000)
                .WithMessage("El comentario no puede exceder los 1000 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.Comentario));
        }
    }
}
