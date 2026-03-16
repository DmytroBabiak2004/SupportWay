using Microsoft.EntityFrameworkCore;
using SupportWay.API.Repositories.Interfaces;
using SupportWay.Data.Context;
using SupportWay.Data.Models;

namespace SupportWay.API.Repositories
{
    public class PostAnalyticsRepository : IPostAnalyticsRepository
    {
        private readonly SupportWayContext _context;

        public PostAnalyticsRepository(SupportWayContext context)
        {
            _context = context;
        }

        public async Task<List<Post>> GetUserPostsAsync(Guid profileId)
        {
            var userId = await _context.Profiles
                .Where(p => p.Id == profileId)
                .Select(p => p.UserId)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(userId))
                return new List<Post>();

            return await _context.Posts
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}