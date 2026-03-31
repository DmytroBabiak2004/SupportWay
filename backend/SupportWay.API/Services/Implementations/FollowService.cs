using SupportWay.Data.Models;
using SupportWay.API.DTOs;
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

        public Task FollowUserAsync(string followerId, string followedId)
            => _followRepository.FollowUserAsync(followerId, followedId);

        public Task UnfollowUserAsync(string followerId, string followedId)
            => _followRepository.UnfollowUserAsync(followerId, followedId);

        public Task<bool> IsFollowingAsync(string followerId, string followedId)
            => _followRepository.IsFollowingAsync(followerId, followedId);

        public Task<int> GetFollowersCountAsync(string userId)
            => _followRepository.GetFollowersCountAsync(userId);

        public Task<int> GetFollowingCountAsync(string userId)
            => _followRepository.GetFollowingCountAsync(userId);

        public async Task<IEnumerable<FollowUserDto>> GetFollowersAsync(string userId)
        {
            var follows = await _followRepository.GetFollowersAsync(userId);
            return follows.Select(f => MapUser(f.Follower, f.Follower.Profile));
        }

        public async Task<IEnumerable<FollowUserDto>> GetFollowingAsync(string userId)
        {
            var follows = await _followRepository.GetFollowingAsync(userId);
            return follows.Select(f => MapUser(f.Followed, f.Followed.Profile));
        }

        public async Task RemoveFollowerAsync(string ownerId, string followerToRemoveId)
        {
            // "ownerId" is the person removing; "followerToRemoveId" followed ownerId
            await _followRepository.UnfollowUserAsync(followerToRemoveId, ownerId);
        }

        private static FollowUserDto MapUser(
            SupportWay.Data.Models.User user,
            SupportWay.Data.Models.Profile? profile) => new()
        {
            UserId    = user.Id,
            Username  = user.UserName ?? string.Empty,
            Name      = profile?.Name,
            FullName  = profile?.FullName,
            PhotoBase64 = profile?.Photo != null ? Convert.ToBase64String(profile.Photo) : null,
            IsVerified  = profile?.IsVerified ?? false,
            VerifiedAs  = profile?.VerifiedAs.HasValue == true ? (int?)profile.VerifiedAs.Value : null
        };
    }
}
