using SupportWay.Data.Repositories.Interfaces;
using SupportWay.Data.Context;
using Microsoft.EntityFrameworkCore;
using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Implementations
{
    public class FollowsRepository : IFollowRepository
    {
        private readonly SupportWayContext _context;

        public FollowsRepository(SupportWayContext context)
        {
            _context = context;
        }

        public async Task FollowUserAsync(string followerId, string followedId)
        {
            if (followerId == followedId) return;

            var existing = await _context.Follows
                .AnyAsync(f => f.FollowerId == followerId && f.FollowedId == followedId);

            if (!existing)
            {
                await _context.Follows.AddAsync(new Follow
                {
                    FollowerId = followerId,
                    FollowedId = followedId,
                    FollowedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
        }

        public async Task UnfollowUserAsync(string followerId, string followedId)
        {
            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowedId == followedId);

            if (follow != null)
            {
                _context.Follows.Remove(follow);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsFollowingAsync(string followerId, string followedId)
            => await _context.Follows
                .AnyAsync(f => f.FollowerId == followerId && f.FollowedId == followedId);

        public async Task<int> GetFollowersCountAsync(string userId)
            => await _context.Follows.CountAsync(f => f.FollowedId == userId);

        public async Task<int> GetFollowingCountAsync(string userId)
            => await _context.Follows.CountAsync(f => f.FollowerId == userId);

        public async Task<IEnumerable<Follow>> GetFollowersAsync(string userId)
            => await _context.Follows
                .Include(f => f.Follower)
                    .ThenInclude(u => u.Profile)
                .Where(f => f.FollowedId == userId)
                .OrderByDescending(f => f.FollowedAt)
                .ToListAsync();

        public async Task<IEnumerable<Follow>> GetFollowingAsync(string userId)
            => await _context.Follows
                .Include(f => f.Followed)
                    .ThenInclude(u => u.Profile)
                .Where(f => f.FollowerId == userId)
                .OrderByDescending(f => f.FollowedAt)
                .ToListAsync();
    }
}
