using Microsoft.EntityFrameworkCore;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.Data.Repositories.Implementations
{
    public class PostLikesRepository : IPostLikesRepoository
    {
        private readonly SupportWayContext _context;

        public PostLikesRepository(SupportWayContext context)
        {
            _context = context;
        }

        public async Task AddPostLikeAsync(Guid postId, string userId)
        {
            var alreadyLiked = await _context.PostLikes
                .AnyAsync(l => l.PostId == postId && l.UserId == userId);

            if (alreadyLiked)
                return;

            var like = new PostLike
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = userId,
                LikedAt = DateTime.UtcNow
            };

            _context.PostLikes.Add(like);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasUserLikedPostAsync(Guid postId, string userId)
        {
            return await _context.PostLikes
                .AnyAsync(l => l.PostId == postId && l.UserId == userId);
        }

        public async Task DeletePostLikeAsync(Guid postId, string userId)
        {
            var likes = await _context.PostLikes
                .Where(l => l.PostId == postId && l.UserId == userId)
                .ToListAsync();

            if (likes.Count == 0)
                return;

            _context.PostLikes.RemoveRange(likes);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetLikesCountAsync(Guid postId)
        {
            return await _context.PostLikes
                .Where(l => l.PostId == postId)
                .Select(l => l.UserId)
                .Distinct()
                .CountAsync();
        }
    }
}
