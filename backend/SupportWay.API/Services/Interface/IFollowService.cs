using SupportWay.API.DTOs;

namespace SupportWay.Services.Interfaces
{
    public interface IFollowService
    {
        Task FollowUserAsync(string followerId, string followedId);
        Task UnfollowUserAsync(string followerId, string followedId);
        Task<bool> IsFollowingAsync(string followerId, string followedId);
        Task<int> GetFollowersCountAsync(string userId);
        Task<int> GetFollowingCountAsync(string userId);
        Task<IEnumerable<FollowUserDto>> GetFollowersAsync(string userId);
        Task<IEnumerable<FollowUserDto>> GetFollowingAsync(string userId);
        Task RemoveFollowerAsync(string ownerId, string followerToRemoveId);
    }
}
