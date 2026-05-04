using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SupportWay.Data.Repositories.Implementations
{
    public class PostsRepository : IPostRepository
    {
        private readonly SupportWayContext _context;

        public PostsRepository(SupportWayContext context)
        {
            _context = context;
        }
        public async Task AddPostAsync(Post post)
        {
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePostAsync(Guid postId)
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
            if (!Guid.TryParse(postId, out var postGuid))
                return null;

            return await _context.Posts
                .AsNoTracking()
                .Include(p => p.User)
                    .ThenInclude(u => u.Profile)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p =>
                    p.Id == postGuid &&
                    EF.Property<string>(p, "PostType") == "Post");
        }


        public async Task<IEnumerable<Post>> GetPostByUserAsync(
            string userId, int pageNumber, int pageSize)
        {
            return await _context.Posts
                .Include(p => p.User)
                    .ThenInclude(u => u.Profile)
                .Include(p => p.Likes)
                .Where(p =>
                    p.UserId == userId &&
                    EF.Property<string>(p, "PostType") == "Post")
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }


        public async Task<IEnumerable<Post>> GetPostsByFollowedUsersAsync(
            string currentUserId, int pageNumber, int pageSize)
        {
            var followedUserIds = await _context.Follows
                .Where(f => f.FollowerId == currentUserId)
                .Select(f => f.FollowedId)
                .ToListAsync();

            return await _context.Posts
                .Include(p => p.User)
                    .ThenInclude(u => u.Profile)
                .Include(p => p.Likes)
                .Where(p =>
                    followedUserIds.Contains(p.UserId) &&
                    EF.Property<string>(p, "PostType") == "Post")
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
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
