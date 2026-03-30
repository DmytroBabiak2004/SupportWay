using Microsoft.EntityFrameworkCore;
using SupportWay.Data.Context;
using SupportWay.Data.DTOs;
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
        await _context.SaveChangesAsync();
    }

    public async Task DeleteHelpRequestAsync(Guid id)
    {
        var helpRequest = await _context.HelpRequests.FindAsync(id);
        if (helpRequest != null)
        {
            _context.HelpRequests.Remove(helpRequest);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<HelpRequest?> GetHelpRequestByIdAsync(Guid helpRequestId)
    {
        return await _context.HelpRequests
            .Include(h => h.User)
            .Include(h => h.Location)
            .Include(h => h.Payments)
            .Include(h => h.Likes)
            .Include(h => h.Comments)
            .Include(h => h.RequestItems)
                .ThenInclude(ri => ri.SupportType)
            .FirstOrDefaultAsync(h => h.Id == helpRequestId);
    }

    public async Task<IEnumerable<HelpRequest>> GetAllHelpRequestsAsync(int pageNumber, int pageSize)
    {
        return await _context.HelpRequests
            .Include(h => h.User)
            .Include(h => h.Location)
            .Include(h => h.Payments)
            .Include(h => h.Likes)
            .Include(h => h.Comments)
            .Include(h => h.RequestItems)
                .ThenInclude(ri => ri.SupportType)
            .OrderByDescending(h => h.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<HelpRequest>> GetHelpRequestsByUserAsync(string userId, int pageNumber, int pageSize)
    {
        return await _context.HelpRequests
            .Include(h => h.User)
            .Include(h => h.Location)
            .Include(h => h.Payments)
            .Include(h => h.Likes)
            .Include(h => h.Comments)
            .Include(h => h.RequestItems)
                .ThenInclude(ri => ri.SupportType)
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
            .Include(h => h.Payments)
            .Include(h => h.Likes)
            .Include(h => h.Comments)
            .Include(h => h.RequestItems)
                .ThenInclude(ri => ri.SupportType)
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

    public async Task<(IEnumerable<HelpRequest> Items, int Total)> GetForMapAsync(
    MapFilterParams filter,
    CancellationToken ct = default)
    {
        IQueryable<HelpRequest> query = _context.HelpRequests
            .AsNoTracking()
            .Include(h => h.Location)
            .Include(h => h.RequestItems)
                .ThenInclude(ri => ri.SupportType)
            .Where(h =>
                (h.Latitude.HasValue && h.Longitude.HasValue) ||
                (h.Location != null && h.Location.Latitude.HasValue && h.Location.Longitude.HasValue));

      
        if (filter.SupportTypeId.HasValue)
            query = query.Where(h =>
                h.RequestItems.Any(ri => ri.SupportTypeId == filter.SupportTypeId.Value));

        if (filter.IsActive.HasValue)
            query = query.Where(h => h.IsActive == filter.IsActive.Value);

       
        if (!string.IsNullOrWhiteSpace(filter.Region))
        {
            var regionLower = filter.Region.ToLower();
            query = query.Where(h =>
                h.Location != null &&
                h.Location.DistrictName.ToLower().Contains(regionLower));
        }

        if (filter.MaxTarget.HasValue)
            query = query.Where(h => h.TargetAmount <= filter.MaxTarget.Value);

        if (filter.MinCollected.HasValue)
            query = query.Where(h => h.CollectedAmount >= filter.MinCollected.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(h => h.CreatedAt)
            .Skip((filter.Page - 1) * filter.Size)
            .Take(filter.Size)
            .ToListAsync(ct); 

        return (items, total);
    }
}