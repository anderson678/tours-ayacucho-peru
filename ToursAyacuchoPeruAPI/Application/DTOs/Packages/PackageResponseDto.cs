using System;

namespace ToursAyacuchoPeruAPI.Application.DTOs.Packages
{
    public interface IPackageWriteDto
    {
        string Nombre { get; set; }
        string Destino { get; set; }
        string? Descripcion { get; set; }
        string? ImagenUrl { get; set; }
        decimal PrecioUnitario { get; set; }
        int CapacidadTotal { get; set; }
        int AsientosDisp { get; set; }
        DateTime FechaInicio { get; set; }
        DateTime FechaFin { get; set; }
    }

    public class PackageResponseDto
    {
        public Guid PaqueteId { get; set; }
        public string Nombre { get; set; } = null!;
        public string Destino { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string? ImagenUrl { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int CapacidadTotal { get; set; }
        public int AsientosDisp { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool Activo { get; set; }
    }

    public class CreatePackageDto : IPackageWriteDto
    {
        public string Nombre { get; set; } = null!;
        public string Destino { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string? ImagenUrl { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int CapacidadTotal { get; set; }
        public int AsientosDisp { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }

    public class UpdatePackageDto : IPackageWriteDto
    {
        public string Nombre { get; set; } = null!;
        public string Destino { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string? ImagenUrl { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int CapacidadTotal { get; set; }
        public int AsientosDisp { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool Activo { get; set; } = true;
    }
}
