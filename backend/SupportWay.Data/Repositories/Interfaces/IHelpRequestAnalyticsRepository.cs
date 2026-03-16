using SupportWay.Data.Models;

namespace SupportWay.API.Repositories.Interfaces
{
    public interface IHelpRequestAnalyticsRepository
    {
        Task<List<HelpRequest>> GetUserHelpRequestsAsync(Guid profileId);
    }
}