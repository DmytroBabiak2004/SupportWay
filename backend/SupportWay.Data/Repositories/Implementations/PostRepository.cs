using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SupportWay.Data.Repositories.Implementations
{
    public class PostRepository : IPostRepository
    {
        private readonly SupportWayContext _context;

        public PostRepository(SupportWayContext context)
        {
            _context = context;
        }
        public async Task AddPostAsync(Post post)
        {
            await _context.Posts.AddAsync(post);
        }

        public async Task DeletePostAsync(int postId)
        {
           var post = await _context.Posts.FindAsync(postId);
            if (post != null)
            {
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Post?> GetPostByIdAsync(string postId)
        {
            return await _context.Posts.FindAsync(postId);
        }

        public async Task<IEnumerable<Post>> GetPostByUserAsync(string userId, int pageNumber, int pageSize)
        {
            return await _context.Posts
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetPostsByFollowedUsersAsync(string currentUserId, int pageNumber, int pageSize)
        {
            var followedUserIds = await _context.Follows
                .Where(f => f.FollowerId == currentUserId)
                .Select(f=>f.FollowedId)
                .ToListAsync();

            return await _context.Posts
                .Where(p => followedUserIds.Contains(p.UserId))
                .OrderByDescending(p=>p.CreatedAt)
                .Skip((pageNumber-1)*pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task UpdatePostAsync(Post post)
        {
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
        }
    }
}
