using System;
using System.Threading.Tasks;
using ToursAyacuchoPeruAPI.Application.DTOs.Reviews;

namespace ToursAyacuchoPeruAPI.Application.Interfaces
{
    public interface IReviewService
    {
        Task<ReviewResponseDto> CreateAsync(Guid clientId, Guid packageId, CreateReviewDto dto);
    }
}
