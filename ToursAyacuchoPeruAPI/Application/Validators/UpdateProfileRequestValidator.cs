// Tarea 3.3 â€” SD-03: Validador UpdateProfileRequestValidator â€” TOURS AYACUCHO PERÃš
// RN-03-02: si se envÃ­a telÃ©fono, debe contener entre 9 y 15 dÃ­gitos numÃ©ricos.
using System;
using FluentValidation;
using ToursAyacuchoPeruAPI.Application.DTOs.Auth;

namespace ToursAyacuchoPeruAPI.Application.Validators
{
    public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileDto>
    {
        public UpdateProfileRequestValidator()
        {
            RuleFor(x => x.Telefono)
                .Matches(@"^\d{9,15}$").WithMessage("El nÃºmero de telÃ©fono debe contener Ãºnicamente entre 9 y 15 dÃ­gitos numÃ©ricos.")
                .When(x => !string.IsNullOrWhiteSpace(x.Telefono));

            RuleFor(x => x.Nombre)
                .MaximumLength(150).WithMessage("El nombre completo no puede exceder los 150 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.Nombre));

            RuleFor(x => x.FotoUrl)
                .MaximumLength(600).WithMessage("La URL de la foto no puede exceder los 600 caracteres.")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var parsed)
                    && (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps))
                .WithMessage("La foto debe ser una URL valida http o https.")
                .When(x => !string.IsNullOrWhiteSpace(x.FotoUrl));
        }
    }
}

