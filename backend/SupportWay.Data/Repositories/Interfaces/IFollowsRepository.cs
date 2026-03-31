using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IFollowRepository
    {
        Task FollowUserAsync(string followerId, string followedId);
        Task UnfollowUserAsync(string followerId, string followedId);
        Task<bool> IsFollowingAsync(string followerId, string followedId);
        Task<int> GetFollowersCountAsync(string userId);
        Task<int> GetFollowingCountAsync(string userId);
        Task<IEnumerable<Follow>> GetFollowersAsync(string userId);
        Task<IEnumerable<Follow>> GetFollowingAsync(string userId);
    }
}
