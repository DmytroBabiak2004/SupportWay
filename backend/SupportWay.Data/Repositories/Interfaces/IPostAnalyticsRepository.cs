using SupportWay.Data.Models;

namespace SupportWay.API.Repositories.Interfaces
{
    public interface IPostAnalyticsRepository
    {
        Task<List<Post>> GetUserPostsAsync(Guid profileId);
    }
}