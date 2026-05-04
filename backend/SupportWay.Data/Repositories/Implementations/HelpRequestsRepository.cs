using Microsoft.EntityFrameworkCore;
using SupportWay.Data.Context;
using SupportWay.Data.DTOs;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

public class HelpRequestsRepository : IHelpRequestsRepository
{
    private readonly SupportWayContext _context;

    public HelpRequestsRepository(SupportWayContext context)
        => _context = context;

    // ─── Існуючі методи без змін ────────────────────────────────────────────

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
                .ThenInclude(u => u.Profile)
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
            .AsNoTracking()
            .Include(h => h.User)
                .ThenInclude(u => u.Profile)
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
            .AsNoTracking()
            .Include(h => h.User)
                .ThenInclude(u => u.Profile)
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
            .AsNoTracking()
            .Include(h => h.User)
                .ThenInclude(u => u.Profile)
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

    // ─── Новий map-markers метод ─────────────────────────────────────────────

    /// <summary>
    /// Projection на рівні EF: SelectMany по RequestItems → MapMarkerDto.
    /// Один результуючий рядок = один RequestItem.
    /// Жодного N+1, жодних EF entities у поверненому результаті.
    /// </summary>
    public async Task<(IEnumerable<MapMarkerDto> Items, int Total)> GetMapMarkersAsync(
        MapFilterParams filter,
        CancellationToken ct = default)
    {
        // Базовий запит по HelpRequest із координатами
        var helpRequestQuery = _context.HelpRequests
            .AsNoTracking()
            .Where(h =>
                (h.Latitude.HasValue && h.Longitude.HasValue) ||
                (h.Location != null && h.Location.Latitude.HasValue && h.Location.Longitude.HasValue));

        // Фільтри по HelpRequest
        if (filter.IsActive.HasValue)
            helpRequestQuery = helpRequestQuery.Where(h => h.IsActive == filter.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(filter.Region))
        {
            var region = filter.Region.ToLower();
            helpRequestQuery = helpRequestQuery.Where(h =>
                h.Location != null &&
                h.Location.DistrictName.ToLower().Contains(region));
        }

        if (filter.MinCollectedAmount.HasValue)
            helpRequestQuery = helpRequestQuery.Where(h => h.CollectedAmount >= filter.MinCollectedAmount.Value);

        if (filter.MaxTargetAmount.HasValue)
            helpRequestQuery = helpRequestQuery.Where(h => h.TargetAmount <= filter.MaxTargetAmount.Value);

        // Projection через SelectMany → RequestItem рівень
        var markersQuery = helpRequestQuery
            .SelectMany(h => h.RequestItems
                .Where(ri => ri.SupportType != null),
                (h, ri) => new MapMarkerDto
                {
                    RequestItemId = ri.Id,
                    HelpRequestId = h.Id,
                    RequestItemName = ri.Name,
                    Quantity = ri.Quantity,
                    UnitPrice = ri.UnitPrice,
                    SupportTypeId = ri.SupportTypeId,
                    SupportTypeName = ri.SupportType!.NameOfType,

                    Latitude = h.Latitude ?? h.Location!.Latitude!.Value,
                    Longitude = h.Longitude ?? h.Location!.Longitude!.Value,
                    LocationName = h.Location != null ? h.Location.DistrictName : string.Empty,
                    LocationAddress = h.Location != null ? h.Location.Address : string.Empty,

                    ShortContent = h.Content.Length > 120 ? h.Content.Substring(0, 120) + "…" : h.Content,

                    TargetAmount = h.TargetAmount,
                    CollectedAmount = h.CollectedAmount,
                    IsActive = h.IsActive,
                    CreatedAt = h.CreatedAt,

                    UserId = h.UserId,
                    UserName = h.User != null ? h.User.UserName : string.Empty,

                    LikesCount = h.Likes != null ? h.Likes.Count : 0,
                    CommentsCount = h.Comments != null ? h.Comments.Count : 0
                });

        // Фільтр по SupportType
        if (filter.SupportTypeId.HasValue)
            markersQuery = markersQuery.Where(m => m.SupportTypeId == filter.SupportTypeId.Value);

        // Пошук по title / content / item name
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            markersQuery = markersQuery.Where(m =>
                m.Title.ToLower().Contains(search) ||
                m.ShortContent.ToLower().Contains(search) ||
                m.RequestItemName.ToLower().Contains(search));
        }

        var total = await markersQuery.CountAsync(ct);

        var items = await markersQuery
            .OrderByDescending(m => m.CreatedAt)
            .Skip((filter.Page - 1) * filter.Size)
            .Take(filter.Size)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<int> CountByUserIdAsync(string userId)
    {
        return await _context.HelpRequests
            .CountAsync(h => h.UserId == userId);
    }
}