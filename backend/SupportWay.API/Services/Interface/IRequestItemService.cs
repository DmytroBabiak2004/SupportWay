using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.Services
{
    public interface IRequestItemService
    {
        public Task<IEnumerable<RequestItem>> GetByHelpRequestIdAsync(Guid helpRequestId);
        public Task<RequestItem> GetByIdAsync(Guid id);

        public Task AddAsync(RequestItem item);

        public Task UpdateAsync(RequestItem item);

        public Task DeleteAsync(Guid id);
    }
}
