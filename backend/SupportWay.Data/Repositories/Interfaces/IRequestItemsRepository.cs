using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IRequestItemsRepository
    {
        Task<IEnumerable<RequestItem>> GetByHelpRequestIdAsync(Guid helpRequestId);
        Task<RequestItem> GetByIdAsync(Guid id);
        Task AddAsync(RequestItem item);
        Task UpdateAsync(RequestItem item);
        Task DeleteAsync(Guid id);
    }
}
