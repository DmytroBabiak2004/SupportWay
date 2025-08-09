using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface ISupportTypesRepository
    {
        Task<IEnumerable<SupportType>> GetAllAsync();
        Task<SupportType> GetByIdAsync(Guid id);
        Task AddAsync(SupportType type);
        Task UpdateAsync(SupportType type);
        Task DeleteAsync(Guid id);
    }
}
