using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IBadgeTypeRepository
    {
        Task<List<BadgeType>> GetAllAsync();
        Task<BadgeType?> GetByIdAsync(Guid id);
        Task<BadgeType?> GetByNameAsync(string name);
        Task AddAsync(BadgeType badgeType);
        void Update(BadgeType badgeType);
        void Delete(BadgeType badgeType);
        Task SaveChangesAsync();
    }
}