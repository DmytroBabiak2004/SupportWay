using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IRequestItemsRepository
    {
        Task<IEnumerable<RequestItem>> GetByHelpRequestIdAsync(int helpRequestId);
        Task<RequestItem> GetByIdAsync(int id);
        Task AddAsync(RequestItem item);
        Task UpdateAsync(RequestItem item);
        Task DeleteAsync(int id);
    }
}
