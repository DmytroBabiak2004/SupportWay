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
            var like = new PostLike
            {
                PostId = postId,
                UserId = userId,
                LikedAt = DateTime.UtcNow
            };
            _context.PostLikes.Add(like);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePostLikeAsync(Guid postId, string userId)
        {
            var like = await  _context.PostLikes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
            if (like != null) 
            { 
                _context.PostLikes.Remove(like);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetLikesCountAsync(Guid postId)
        {
            return await _context.PostLikes
                .Where(l => l.PostId == postId)
                .CountAsync();
        }
    }
}
