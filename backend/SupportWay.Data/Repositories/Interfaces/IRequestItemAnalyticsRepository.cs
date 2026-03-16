using SupportWay.Data.Models;

namespace SupportWay.API.Repositories.Interfaces
{
    public interface IRequestItemAnalyticsRepository
    {
        Task<List<RequestItem>> GetUserRequestItemsAsync(Guid profileId);
    }
}