using SupportWay.API.Repositories.Interfaces;
using SupportWay.API.Services.Interfaces;

namespace SupportWay.API.Services
{
    public class ProfileAnalyticsService : IProfileAnalyticsService
    {
        private readonly IPostAnalyticsRepository _postAnalyticsRepository;
        private readonly IHelpRequestAnalyticsRepository _helpRequestAnalyticsRepository;
        private readonly IRequestItemAnalyticsRepository _requestItemAnalyticsRepository;

        public ProfileAnalyticsService(
            IPostAnalyticsRepository postAnalyticsRepository,
            IHelpRequestAnalyticsRepository helpRequestAnalyticsRepository,
            IRequestItemAnalyticsRepository requestItemAnalyticsRepository)
        {
            _postAnalyticsRepository = postAnalyticsRepository;
            _helpRequestAnalyticsRepository = helpRequestAnalyticsRepository;
            _requestItemAnalyticsRepository = requestItemAnalyticsRepository;
        }

        public async Task<object> GetDashboardAsync(Guid profileId)
        {
            var posts = await _postAnalyticsRepository.GetUserPostsAsync(profileId);
            var requests = await _helpRequestAnalyticsRepository.GetUserHelpRequestsAsync(profileId);
            var requestItems = await _requestItemAnalyticsRepository.GetUserRequestItemsAsync(profileId);

            var postsByDate = posts
                .GroupBy(x => x.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            var requestsActivity = requests
                .GroupBy(x => x.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            var requestsByType = requestItems
                .Where(x => x.SupportType != null)
                .GroupBy(x => new { x.SupportTypeId, x.SupportType.NameOfType })
                .Select(g => new
                {
                    TypeId = g.Key.SupportTypeId,
                    TypeName = g.Key.NameOfType,
                    ItemsCount = g.Count(),
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .ToList();

            var itemsByName = requestItems
                .GroupBy(x => x.Name)
                .Select(g => new
                {
                    ItemName = g.Key,
                    Count = g.Count(),
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .ToList();

            var itemsBySupportType = requestItems
                .Where(x => x.SupportType != null)
                .GroupBy(x => new { x.SupportTypeId, x.SupportType.NameOfType })
                .Select(g => new
                {
                    TypeId = g.Key.SupportTypeId,
                    TypeName = g.Key.NameOfType,
                    ItemsCount = g.Count(),
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .ToList();

            return new
            {
                TotalPosts = posts.Count,
                TotalRequests = requests.Count,
                TotalRequestItems = requestItems.Count,

                PostsByDate = postsByDate,
                RequestsActivity = requestsActivity,

                RequestsByType = requestsByType,
                ItemsBySupportType = itemsBySupportType,
                MostUsedItems = itemsByName,

                DominantRequestType = requestsByType.FirstOrDefault(),
                MostRequestedItem = itemsByName.FirstOrDefault(),

                LastPostDate = posts.OrderBy(x => x.CreatedAt).LastOrDefault()?.CreatedAt,
                FirstPostDate = posts.OrderBy(x => x.CreatedAt).FirstOrDefault()?.CreatedAt,
                LastRequestDate = requests.OrderBy(x => x.CreatedAt).LastOrDefault()?.CreatedAt,
                FirstRequestDate = requests.OrderBy(x => x.CreatedAt).FirstOrDefault()?.CreatedAt
            };
        }
    }
}