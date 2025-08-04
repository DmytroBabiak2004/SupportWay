using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.Services
{
    public interface IRequestItemService
    {
        public Task<IEnumerable<RequestItem>> GetByHelpRequestIdAsync(int helpRequestId);
        public Task<RequestItem> GetByIdAsync(int id);

        public Task AddAsync(RequestItem item);

        public Task UpdateAsync(RequestItem item);

        public Task DeleteAsync(int id);
    }
}
