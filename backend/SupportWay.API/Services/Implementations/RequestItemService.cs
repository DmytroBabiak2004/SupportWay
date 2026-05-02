using SupportWay.API.Services.Interfaces;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.Services
{
    public class RequestItemService : IRequestItemService
    {
        private readonly IRequestItemsRepository _repository;
        private readonly IBadgeAwardService _badgeAwardService;
        private readonly ILogger<RequestItemService> _logger;

        public RequestItemService(
            IRequestItemsRepository repository,
            IBadgeAwardService badgeAwardService,
            ILogger<RequestItemService> logger)
        {
            _repository = repository;
            _badgeAwardService = badgeAwardService;
            _logger = logger;
        }

        public Task<IEnumerable<RequestItem>> GetByHelpRequestIdAsync(Guid helpRequestId) =>
            _repository.GetByHelpRequestIdAsync(helpRequestId);

        public Task<RequestItem> GetByIdAsync(Guid id) =>
            _repository.GetByIdAsync(id);

        public async Task AddAsync(RequestItem item)
        {
            await _repository.AddAsync(item);

            try
            {
                await _badgeAwardService.CheckAndAwardRequestItemBadgesAsync(item.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "RequestItemService: не вдалося перевірити нагороди після створення RequestItem {RequestItemId}.",
                    item.Id);
            }
        }

        public async Task UpdateAsync(RequestItem item)
        {
            await _repository.UpdateAsync(item);

            try
            {
                await _badgeAwardService.CheckAndAwardRequestItemBadgesAsync(item.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "RequestItemService: не вдалося перевірити нагороди після оновлення RequestItem {RequestItemId}.",
                    item.Id);
            }
        }

        public Task DeleteAsync(Guid id) =>
            _repository.DeleteAsync(id);
    }
}
