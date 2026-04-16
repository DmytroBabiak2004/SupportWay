using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IProfileBadgeRepository
    {
        Task<ProfileBadge?> GetByProfileAndBadgeAsync(Guid profileId, Guid badgeId);
        Task<bool> ExistsAsync(Guid profileId, Guid badgeId);
        Task AddAsync(ProfileBadge profileBadge);
        void Delete(ProfileBadge profileBadge);
        Task SaveChangesAsync();
    }
}