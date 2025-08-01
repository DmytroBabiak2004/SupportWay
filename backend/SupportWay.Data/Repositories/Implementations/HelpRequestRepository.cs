using Microsoft.EntityFrameworkCore;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.Data.Repositories.Implementations
{

    public class HelpRequestRepository : IHelpRequestsRepository
    {
        private readonly SupportWayContext _context;
        public HelpRequestRepository(SupportWayContext context)
        {
            _context = context;
        }
        public async Task AddHelpRequestAsync(HelpRequest helpRequest)
        {
             await _context.HelpRequests.AddAsync(helpRequest);
        }

        public async Task DeleteHelpRequestAsync(int id)
        {
            var helpRequest = await _context.HelpRequests.FindAsync(id);
            if (helpRequest != null)
            {
                 _context.HelpRequests.Remove(helpRequest);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<HelpRequest>> GetHelpRequestsByFollowedUsersAsync(string currentUserId, int pageNumber, int pageSize)
        {
            var followedUserIds = await _context.Follows
                .Where(f => f.FollowerId == currentUserId)
                .Select(f => f.FollowedId)
                .ToListAsync();

            return await _context.HelpRequests
                .Where(h => followedUserIds.Contains(h.User.Id))
                .OrderByDescending(h => h.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        }

        public async Task<HelpRequest> GetHelpRequestByIdAsync(int helpRequestid)
        {
            return await _context.HelpRequests.FindAsync(helpRequestid);
        }

        public async Task<IEnumerable<HelpRequest>> GetHelpRequestsByUserAsync(string userId, int pageNumber, int pageSize)
        {
            return await _context.HelpRequests
                .Where(h => userId.Contains(h.User.Id))
                .OrderByDescending(h => h.CreatedAt)
                .Skip((pageNumber - 1) * pageNumber)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task UpdateHelpRequestAsync(HelpRequest helpRequest)
        {
            _context.HelpRequests.Update(helpRequest);
            await _context.SaveChangesAsync();
        }
    }
}
