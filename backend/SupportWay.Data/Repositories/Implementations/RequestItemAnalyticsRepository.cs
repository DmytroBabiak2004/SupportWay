using Microsoft.EntityFrameworkCore;
using SupportWay.API.Repositories.Interfaces;
using SupportWay.Data.Context;
using SupportWay.Data.Models;

namespace SupportWay.API.Repositories
{
    public class RequestItemAnalyticsRepository : IRequestItemAnalyticsRepository
    {
        private readonly SupportWayContext _context;

        public RequestItemAnalyticsRepository(SupportWayContext context)
        {
            _context = context;
        }

        public async Task<List<RequestItem>> GetUserRequestItemsAsync(Guid profileId)
        {
            var userId = await _context.Profiles
                .Where(p => p.Id == profileId)
                .Select(p => p.UserId)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(userId))
                return new List<RequestItem>();

            return await _context.RequestItems
                .Include(x => x.HelpRequest)
                .Include(x => x.SupportType)
                .Where(x => x.HelpRequest.UserId == userId)
                .ToListAsync();
        }
    }
}