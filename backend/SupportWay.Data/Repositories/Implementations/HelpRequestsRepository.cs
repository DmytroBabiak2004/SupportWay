using Microsoft.EntityFrameworkCore;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

public class HelpRequestsRepository : IHelpRequestsRepository
{
    private readonly SupportWayContext _context;

    public HelpRequestsRepository(SupportWayContext context)
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

    public async Task<HelpRequest?> GetHelpRequestByIdAsync(int helpRequestId)
    {
        return await _context.HelpRequests
            .Include(h => h.User)
            .Include(h => h.Location)
            .Include(h => h.RequestStatus)
            .Include(h => h.Payments)
            .FirstOrDefaultAsync(h => h.Id == helpRequestId);
    }

    public async Task<IEnumerable<HelpRequest>> GetHelpRequestsByUserAsync(string userId, int pageNumber, int pageSize)
    {
        return await _context.HelpRequests
            .Include(h => h.User)
            .Include(h => h.Location)
            .Include(h => h.RequestStatus)
            .Include(h => h.Payments)
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<HelpRequest>> GetHelpRequestsByFollowedUsersAsync(string currentUserId, int pageNumber, int pageSize)
    {
        var followedUserIds = await _context.Follows
            .Where(f => f.FollowerId == currentUserId)
            .Select(f => f.FollowedId)
            .ToListAsync();

        return await _context.HelpRequests
            .Include(h => h.User)
            .Include(h => h.Location)
            .Include(h => h.RequestStatus)
            .Include(h => h.Payments)
            .Where(h => followedUserIds.Contains(h.UserId))
            .OrderByDescending(h => h.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task UpdateHelpRequestAsync(HelpRequest helpRequest)
    {
        _context.HelpRequests.Update(helpRequest);
        await _context.SaveChangesAsync();
    }
}
