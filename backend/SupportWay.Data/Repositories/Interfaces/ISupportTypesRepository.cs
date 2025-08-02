using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface ISupportTypesRepository
    {
        Task<IEnumerable<SupportType>> GetAllAsync();
        Task<SupportType> GetByIdAsync(int id);
        Task AddAsync(SupportType type);
        Task UpdateAsync(SupportType type);
        Task DeleteAsync(int id);
    }
}
