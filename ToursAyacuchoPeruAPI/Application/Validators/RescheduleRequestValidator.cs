using FluentValidation;
using ToursAyacuchoPeruAPI.Application.DTOs.Reservations;

namespace ToursAyacuchoPeruAPI.Application.Validators
{
    public class RescheduleRequestValidator : AbstractValidator<RescheduleRequestDto>
    {
        public RescheduleRequestValidator()
        {
            RuleFor(x => x.NuevaFecha)
                .Must(fecha => fecha != default)
                .WithMessage("La nueva fecha de la reserva es requerida.");
        }
    }
}
