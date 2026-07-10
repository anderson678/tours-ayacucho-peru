using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ToursAyacuchoPeruAPI.Application.DTOs.Packages;
using ToursAyacuchoPeruAPI.Application.Interfaces;
using ToursAyacuchoPeruAPI.Domain.Entities;
using ToursAyacuchoPeruAPI.Domain.Exceptions;
using ToursAyacuchoPeruAPI.Infrastructure.Persistence;

namespace ToursAyacuchoPeruAPI.Application.Services
{
    public class PackageService : IPackageService
    {
        private readonly ToursAyacuchoPeruDbContext _db;

        public PackageService(ToursAyacuchoPeruDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<PackageResponseDto>> GetActivePackagesAsync()
        {
            return await _db.PaquetesTuristicos
                .Where(p => p.Activo)
                .AsNoTracking()
                .Select(p => ToResponseExpression(p))
                .ToListAsync();
        }

        public async Task<IEnumerable<PackageResponseDto>> GetAllPackagesAsync()
        {
            return await _db.PaquetesTuristicos
                .AsNoTracking()
                .OrderByDescending(p => p.Activo)
                .ThenBy(p => p.FechaInicio)
                .Select(p => ToResponseExpression(p))
                .ToListAsync();
        }

        public async Task<PackageResponseDto> GetByIdAsync(Guid packageId)
        {
            var paquete = await _db.PaquetesTuristicos
                .AsNoTracking()
                .Where(p => p.PaqueteId == packageId)
                .Select(p => ToResponseExpression(p))
                .FirstOrDefaultAsync();

            return paquete ?? throw new NotFoundException("Paquete no encontrado");
        }

        public async Task<PackageResponseDto> CreateAsync(CreatePackageDto dto)
        {
            var paquete = new PaqueteTuristico
            {
                PaqueteId = Guid.NewGuid(),
                Nombre = dto.Nombre.Trim(),
                Destino = dto.Destino.Trim(),
                Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
                ImagenUrl = string.IsNullOrWhiteSpace(dto.ImagenUrl) ? null : dto.ImagenUrl.Trim(),
                PrecioUnitario = dto.PrecioUnitario,
                CapacidadTotal = dto.CapacidadTotal,
                AsientosDisp = dto.AsientosDisp,
                FechaInicio = dto.FechaInicio.Date,
                FechaFin = dto.FechaFin.Date,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            _db.PaquetesTuristicos.Add(paquete);
            await _db.SaveChangesAsync();

            return ToResponse(paquete);
        }

        public async Task<PackageResponseDto> UpdateAsync(Guid packageId, UpdatePackageDto dto)
        {
            var paquete = await _db.PaquetesTuristicos.FindAsync(packageId)
                ?? throw new NotFoundException("Paquete no encontrado");

            paquete.Nombre = dto.Nombre.Trim();
            paquete.Destino = dto.Destino.Trim();
            paquete.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();
            paquete.ImagenUrl = string.IsNullOrWhiteSpace(dto.ImagenUrl) ? null : dto.ImagenUrl.Trim();
            paquete.PrecioUnitario = dto.PrecioUnitario;
            paquete.CapacidadTotal = dto.CapacidadTotal;
            paquete.AsientosDisp = dto.AsientosDisp;
            paquete.FechaInicio = dto.FechaInicio.Date;
            paquete.FechaFin = dto.FechaFin.Date;
            paquete.Activo = dto.Activo;

            _db.PaquetesTuristicos.Update(paquete);
            await _db.SaveChangesAsync();

            return ToResponse(paquete);
        }

        public async Task DeactivateAsync(Guid packageId)
        {
            var paquete = await _db.PaquetesTuristicos.FindAsync(packageId)
                ?? throw new NotFoundException("Paquete no encontrado");

            paquete.Activo = false;
            _db.PaquetesTuristicos.Update(paquete);
            await _db.SaveChangesAsync();
        }

        private static PackageResponseDto ToResponse(PaqueteTuristico paquete)
        {
            return new PackageResponseDto
            {
                PaqueteId = paquete.PaqueteId,
                Nombre = paquete.Nombre,
                Destino = paquete.Destino,
                Descripcion = paquete.Descripcion,
                ImagenUrl = paquete.ImagenUrl,
                PrecioUnitario = paquete.PrecioUnitario,
                CapacidadTotal = paquete.CapacidadTotal,
                AsientosDisp = paquete.AsientosDisp,
                FechaInicio = paquete.FechaInicio,
                FechaFin = paquete.FechaFin,
                Activo = paquete.Activo
            };
        }

        private static PackageResponseDto ToResponseExpression(PaqueteTuristico paquete)
        {
            return new PackageResponseDto
            {
                PaqueteId = paquete.PaqueteId,
                Nombre = paquete.Nombre,
                Destino = paquete.Destino,
                Descripcion = paquete.Descripcion,
                ImagenUrl = paquete.ImagenUrl,
                PrecioUnitario = paquete.PrecioUnitario,
                CapacidadTotal = paquete.CapacidadTotal,
                AsientosDisp = paquete.AsientosDisp,
                FechaInicio = paquete.FechaInicio,
                FechaFin = paquete.FechaFin,
                Activo = paquete.Activo
            };
        }
    }
}
