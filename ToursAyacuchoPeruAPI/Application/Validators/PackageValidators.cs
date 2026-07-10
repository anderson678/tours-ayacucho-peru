using FluentValidation;
using ToursAyacuchoPeruAPI.Application.DTOs.Packages;

namespace ToursAyacuchoPeruAPI.Application.Validators
{
    public class CreatePackageValidator : AbstractValidator<CreatePackageDto>
    {
        public CreatePackageValidator()
        {
            Include(new PackageFieldsValidator<CreatePackageDto>());
        }
    }

    public class UpdatePackageValidator : AbstractValidator<UpdatePackageDto>
    {
        public UpdatePackageValidator()
        {
            Include(new PackageFieldsValidator<UpdatePackageDto>());
        }
    }

    internal class PackageFieldsValidator<T> : AbstractValidator<T>
        where T : IPackageWriteDto
    {
        public PackageFieldsValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre del paquete es requerido.")
                .MaximumLength(200).WithMessage("El nombre no puede exceder los 200 caracteres.");

            RuleFor(x => x.Destino)
                .NotEmpty().WithMessage("El destino del paquete es requerido.")
                .MaximumLength(200).WithMessage("El destino no puede exceder los 200 caracteres.");

            RuleFor(x => x.Descripcion)
                .MaximumLength(2000).WithMessage("La descripción no puede exceder los 2000 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.Descripcion));

            RuleFor(x => x.ImagenUrl)
                .MaximumLength(600).WithMessage("La URL de la imagen no puede exceder los 600 caracteres.")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var parsed)
                    && (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps))
                .WithMessage("La URL de la imagen debe ser una URL HTTP o HTTPS valida.")
                .When(x => !string.IsNullOrWhiteSpace(x.ImagenUrl));
            RuleFor(x => x.PrecioUnitario)
                .GreaterThan(0).WithMessage("El precio del paquete debe ser mayor a 0.");

            RuleFor(x => x.CapacidadTotal)
                .GreaterThan(0).WithMessage("La capacidad total debe ser un entero positivo.");

            RuleFor(x => x.AsientosDisp)
                .GreaterThanOrEqualTo(0).WithMessage("Los asientos disponibles no pueden ser negativos.")
                .LessThanOrEqualTo(x => x.CapacidadTotal).WithMessage("Los asientos disponibles no pueden exceder la capacidad total.");

            RuleFor(x => x.FechaInicio)
                .Must(fecha => fecha != default).WithMessage("La fecha de inicio es requerida.");

            RuleFor(x => x.FechaFin)
                .Must(fecha => fecha != default).WithMessage("La fecha de fin es requerida.")
                .GreaterThanOrEqualTo(x => x.FechaInicio).WithMessage("La fecha de fin debe ser posterior o igual a la fecha de inicio.");
        }
    }
}

