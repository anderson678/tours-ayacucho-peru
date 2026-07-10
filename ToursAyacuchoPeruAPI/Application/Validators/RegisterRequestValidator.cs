// Tarea 2.2 â€” SD-01: Validador RegisterRequestValidator â€” TOURS AYACUCHO PERÃš
using System.Linq;
using FluentValidation;
using ToursAyacuchoPeruAPI.Application.DTOs.Auth;

namespace ToursAyacuchoPeruAPI.Application.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre completo es requerido.")
                .MaximumLength(150).WithMessage("El nombre completo no puede exceder los 150 caracteres.");

            RuleFor(x => x.Correo)
                .NotEmpty().WithMessage("El correo electrÃ³nico es requerido.")
                .EmailAddress().WithMessage("El correo electrÃ³nico no tiene un formato vÃ¡lido (RFC 5322).")
                .MaximumLength(254).WithMessage("El correo electrÃ³nico no puede exceder los 254 caracteres.");

            // RN-01-02: mÃ­nimo 8 caracteres, al menos una mayÃºscula, un dÃ­gito y un carÃ¡cter especial.
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseÃ±a es requerida.")
                .MinimumLength(8).WithMessage("La contraseÃ±a debe tener una longitud mÃ­nima de 8 caracteres.")
                .Must(p => p != null && p.Any(char.IsUpper)).WithMessage("La contraseÃ±a debe contener al menos una letra mayÃºscula.")
                .Must(p => p != null && p.Any(char.IsDigit)).WithMessage("La contraseÃ±a debe contener al menos un dÃ­gito.")
                .Must(p => p != null && p.Any(c => !char.IsLetterOrDigit(c))).WithMessage("La contraseÃ±a debe contener al menos un carÃ¡cter especial.");

            // RN-03-02 (formato de telÃ©fono compartido con actualizaciÃ³n de perfil)
            RuleFor(x => x.Telefono)
                .NotEmpty().WithMessage("El nÃºmero de telÃ©fono es requerido.")
                .Matches(@"^\d{9,15}$").WithMessage("El nÃºmero de telÃ©fono debe contener Ãºnicamente entre 9 y 15 dÃ­gitos numÃ©ricos.");
        }
    }
}

