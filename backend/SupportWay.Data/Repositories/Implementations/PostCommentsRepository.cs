using Microsoft.EntityFrameworkCore;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupportWay.Data.Repositories.Implementations
{
    public class PostCommentsRepository : IPostCommentsRepository
    {
        private readonly SupportWayContext _context;

        public PostCommentsRepository(SupportWayContext context)
        {
            _context = context;
        }

        public async Task AddCommentAsync(PostComment comment)
        {
            _context.PostComments.Add(comment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCommentAsync(Guid id)
        {
            var comment = await _context.PostComments.FindAsync(id);
            if (comment != null)
            {
                _context.PostComments.Remove(comment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<PostComment> GetCommentByIdAsync(Guid id)
        {
            return await _context.PostComments
                .Include(c => c.User)
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<PostComment>> GetCommentsByPostAsync(Guid postId)
        {
            return await _context.PostComments
                .Where(c => c.PostId == postId)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PostComment>> GetCommentsByUserAsync(string userId)
        {
            return await _context.PostComments
                .Where(c => c.UserId == userId)
                .Include(c => c.Post)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}
