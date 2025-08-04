namespace SupportWay.Services.Interfaces
{
    public interface IFollowService
    {
        Task FollowUserAsync(string followerId, string followedId);
        Task UnfollowUserAsync(string followerId, string followedId);
        Task<bool> IsFollowingAsync(string followerId, string followedId);
        Task<int> GetFollowersCountAsync(string userId);
        Task<int> GetFollowingCountAsync(string userId);
    }
}
