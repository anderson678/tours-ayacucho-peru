using FluentValidation;
using ToursAyacuchoPeruAPI.Application.DTOs.Reservations;

namespace ToursAyacuchoPeruAPI.Application.Validators
{
    public class CreateReservationValidator : AbstractValidator<CreateReservationDto>
    {
        public CreateReservationValidator()
        {
            RuleFor(x => x.PaqueteId)
                .NotEmpty().WithMessage("El identificador del paquete turístico es requerido.");

            RuleFor(x => x.CantAsientos)
                .GreaterThanOrEqualTo(1).WithMessage("La cantidad de asientos solicitados debe ser al menos 1.");

            RuleFor(x => x.FechaInicio)
                .Must(fecha => fecha != default)
                .WithMessage("La fecha de inicio de la reserva es requerida.");
        }
    }
}
