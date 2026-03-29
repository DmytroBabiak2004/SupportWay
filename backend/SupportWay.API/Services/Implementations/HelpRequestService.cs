using SupportWay.Data.DTOs;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

public class HelpRequestService : IHelpRequestService
{
    private readonly IHelpRequestsRepository _helpRepo;
    private readonly ILocationRepository _locationRepo;

    public HelpRequestService(
        IHelpRequestsRepository helpRepo,
        ILocationRepository locationRepo)
    {
        _helpRepo = helpRepo;
        _locationRepo = locationRepo;
    }

    public async Task<IEnumerable<HelpRequestDto>> GetUserHelpRequestsAsync(string userId, int page, int size)
    {
        var requests = await _helpRepo.GetHelpRequestsByUserAsync(userId, page, size);

        var result = new List<HelpRequestDto>();
        foreach (var request in requests)
        {
            result.Add(MapToDto(request));
        }

        return result;
    }

    public async Task<HelpRequestDto?> GetHelpRequestByIdAsync(Guid id)
    {
        var request = await _helpRepo.GetHelpRequestByIdAsync(id);
        if (request == null)
            return null;

        return MapToDto(request);
    }

    public async Task CreateHelpRequestAsync(HelpRequestCreateDto dto, string userId)
    {
        var helpRequest = new HelpRequest
        {
            Id = Guid.NewGuid(),
            Content = dto.Content,
            Image = dto.Image,
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            LocationId = dto.LocationId
        };

        await _helpRepo.AddHelpRequestAsync(helpRequest);
    }

    public async Task DeleteHelpRequestAsync(Guid id)
    {
        await _helpRepo.DeleteHelpRequestAsync(id);
    }

    private HelpRequestDto MapToDto(HelpRequest r)
    {
        var requestItems = new List<SupportWay.Data.DTOs.RequestItemDto>();

        if (r.RequestItems != null)
        {
            foreach (var item in r.RequestItems)
            {
                requestItems.Add(MapRequestItemToDto(item));
            }
        }

        return new HelpRequestDto
        {
            Id = r.Id,
            Content = r.Content ?? string.Empty,
            Image = r.Image,
            CreatedAt = r.CreatedAt,
            UserId = r.UserId,
            UserName = r.User?.UserName ?? "Unknown",
            LikesCount = r.Likes?.Count ?? 0,
            CommentsCount = r.Comments?.Count ?? 0,
            LocationId = r.LocationId,
            LocationName = r.Location?.DistrictName ?? string.Empty,
            TotalPayments = r.Payments?.Sum(p => p.Amount) ?? 0,
            RequestItems = requestItems
        };
    }

    private SupportWay.Data.DTOs.RequestItemDto MapRequestItemToDto(RequestItem item)
    {
        return new SupportWay.Data.DTOs.RequestItemDto
        {
            Id = item.Id,
            HelpRequestId = item.HelpRequestId,
            SupportTypeId = item.SupportTypeId,
            Name = item.Name ?? string.Empty,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            SupportTypeName = item.SupportType?.NameOfType ?? string.Empty
        };
    }
}