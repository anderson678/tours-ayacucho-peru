using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToursAyacuchoPeruAPI.Application.DTOs.Packages;

namespace ToursAyacuchoPeruAPI.Application.Interfaces
{
    public interface IPackageService
    {
        Task<IEnumerable<PackageResponseDto>> GetActivePackagesAsync();
        Task<IEnumerable<PackageResponseDto>> GetAllPackagesAsync();
        Task<PackageResponseDto> GetByIdAsync(Guid packageId);
        Task<PackageResponseDto> CreateAsync(CreatePackageDto dto);
        Task<PackageResponseDto> UpdateAsync(Guid packageId, UpdatePackageDto dto);
        Task DeactivateAsync(Guid packageId);
    }
}
