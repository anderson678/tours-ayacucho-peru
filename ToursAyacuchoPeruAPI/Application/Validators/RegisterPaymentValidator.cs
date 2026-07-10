using FluentValidation;
using ToursAyacuchoPeruAPI.Application.DTOs.Reservations;

namespace ToursAyacuchoPeruAPI.Application.Validators
{
    public class RegisterPaymentValidator : AbstractValidator<RegisterPaymentDto>
    {
        public RegisterPaymentValidator()
        {
            RuleFor(x => x.ReservaId)
                .NotEmpty().WithMessage("El identificador de la reserva es requerido.");

            RuleFor(x => x.Monto)
                .GreaterThan(0).WithMessage("El monto del pago debe ser mayor a 0.");

            RuleFor(x => x.MetodoPago)
                .IsInEnum().WithMessage("El metodo de pago no es valido. Metodos aceptados: TransferenciaBancaria, DepositoCuenta, Yape, Plin.");

            RuleFor(x => x.NumReferencia)
                .NotEmpty().WithMessage("El numero de referencia es requerido.")
                .MaximumLength(100).WithMessage("El numero de referencia no puede exceder los 100 caracteres.");

            RuleFor(x => x.ComprobanteArchivoNombre)
                .MaximumLength(180).WithMessage("El nombre del archivo no puede exceder los 180 caracteres.");

            RuleFor(x => x.ComprobanteArchivoTipo)
                .MaximumLength(80).WithMessage("El tipo del archivo no puede exceder los 80 caracteres.");

            RuleFor(x => x.ComprobanteArchivoBase64)
                .MaximumLength(2_800_000).WithMessage("El comprobante adjunto no puede exceder 2 MB.");
        }
    }
}
