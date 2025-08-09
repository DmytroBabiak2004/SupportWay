using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.Services
{
    public class RequestItemService : IRequestItemService
    {
        private readonly IRequestItemsRepository _repository;

        public RequestItemService(IRequestItemsRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<RequestItem>> GetByHelpRequestIdAsync(Guid helpRequestId) =>
            _repository.GetByHelpRequestIdAsync(helpRequestId);

        public Task<RequestItem> GetByIdAsync(Guid id) =>
            _repository.GetByIdAsync(id);

        public Task AddAsync(RequestItem item) =>
            _repository.AddAsync(item);

        public Task UpdateAsync(RequestItem item) =>
            _repository.UpdateAsync(item);

        public Task DeleteAsync(Guid id) =>
            _repository.DeleteAsync(id);
    }
}
