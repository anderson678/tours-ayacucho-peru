// Tarea 3.1 â€” SD-02: Validador LoginRequestValidator â€” TOURS AYACUCHO PERÃš
using FluentValidation;
using ToursAyacuchoPeruAPI.Application.DTOs.Auth;

namespace ToursAyacuchoPeruAPI.Application.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Correo)
                .NotEmpty().WithMessage("El correo electrÃ³nico es requerido.")
                .EmailAddress().WithMessage("El correo electrÃ³nico no tiene un formato vÃ¡lido.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseÃ±a es requerida.");
        }
    }
}

