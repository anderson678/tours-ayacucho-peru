using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToursAyacuchoPeruAPI.Application.DTOs.Admin;

namespace ToursAyacuchoPeruAPI.Application.Interfaces
{
    public interface IAdminClientService
    {
        Task<IEnumerable<AdminClientResponseDto>> GetClientsAsync();
        Task<AdminClientResponseDto> UpdateClientStatusAsync(Guid clientId, UpdateClientStatusDto dto);
    }
}
