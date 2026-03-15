using SupportWay.Data.Models;

namespace SupportWay.API.Repositories.Interfaces
{
    public interface IBadgeRepository
    {
        Task<List<Badge>> GetAllAsync();
        Task<Badge?> GetByIdAsync(Guid id);
        Task<List<Badge>> GetByProfileIdAsync(Guid profileId);
        Task<bool> BadgeTypeExistsAsync(Guid badgeTypeId);
        Task AddAsync(Badge badge);
        void Delete(Badge badge);
        Task SaveChangesAsync();
    }
}