using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface ILocationsRepository
    {
        Task<Location> GetByIdAsync(Guid? id);
        Task<IEnumerable<Location>> GetAllAsync();
        Task AddAsync(Location location);
        Task UpdateAsync(Location location);
        Task DeleteAsync(Guid id);
    }
}
