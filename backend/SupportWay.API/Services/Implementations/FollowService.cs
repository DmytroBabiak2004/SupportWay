using SupportWay.Data.Repositories.Interfaces;
using SupportWay.Services.Interfaces;

namespace SupportWay.Services.Implementations
{
    public class FollowService : IFollowService
    {
        private readonly IFollowRepository _followRepository;

        public FollowService(IFollowRepository followRepository)
        {
            _followRepository = followRepository;
        }

        public async Task FollowUserAsync(string followerId, string followedId)
        {
            await _followRepository.FollowUserAsync(followerId, followedId);
        }

        public async Task UnfollowUserAsync(string followerId, string followedId)
        {
            await _followRepository.UnfollowUserAsync(followerId, followedId);
        }

        public async Task<bool> IsFollowingAsync(string followerId, string followedId)
        {
            return await _followRepository.IsFollowingAsync(followerId, followedId);
        }

        public async Task<int> GetFollowersCountAsync(string userId)
        {
            return await _followRepository.GetFollowersCountAsync(userId);
        }

        public async Task<int> GetFollowingCountAsync(string userId)
        {
            return await _followRepository.GetFollowingCountAsync(userId);
        }
    }
}
