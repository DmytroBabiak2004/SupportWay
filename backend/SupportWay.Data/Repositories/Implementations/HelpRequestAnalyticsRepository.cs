using Microsoft.EntityFrameworkCore;
using SupportWay.API.Repositories.Interfaces;
using SupportWay.Data.Context;
using SupportWay.Data.Models;

namespace SupportWay.API.Repositories
{
    public class HelpRequestAnalyticsRepository : IHelpRequestAnalyticsRepository
    {
        private readonly SupportWayContext _context;

        public HelpRequestAnalyticsRepository(SupportWayContext context)
        {
            _context = context;
        }

        public async Task<List<HelpRequest>> GetUserHelpRequestsAsync(Guid profileId)
        {
            var userId = await _context.Profiles
                .Where(p => p.Id == profileId)
                .Select(p => p.UserId)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(userId))
                return new List<HelpRequest>();

            return await _context.HelpRequests
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}