using SupportWay.API.DTOs;

namespace SupportWay.API.Services.Interfaces
{
    public interface IBadgeService
    {
        Task<List<BadgeResponse>> GetAllAsync();
        Task<BadgeResponse?> GetByIdAsync(Guid id);
        Task<List<BadgeResponse>> GetByProfileIdAsync(Guid profileId);
        Task<Guid> CreateBadgeAsync(CreateBadgeRequest request);
        Task UpdateBadgeAsync(UpdateBadgeRequest request);
        Task DeleteBadgeAsync(Guid id);
    }
}