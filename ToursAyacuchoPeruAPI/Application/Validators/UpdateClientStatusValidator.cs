using FluentValidation;
using ToursAyacuchoPeruAPI.Application.DTOs.Admin;
using ToursAyacuchoPeruAPI.Domain.Enums;

namespace ToursAyacuchoPeruAPI.Application.Validators
{
    public class UpdateClientStatusValidator : AbstractValidator<UpdateClientStatusDto>
    {
        public UpdateClientStatusValidator()
        {
            RuleFor(x => x.Estado)
                .Must(estado => estado is EstadoUsuario.Activo or EstadoUsuario.Inactivo)
                .WithMessage("El estado solicitado no es válido. Estados permitidos: Activo, Inactivo.");
        }
    }
}
