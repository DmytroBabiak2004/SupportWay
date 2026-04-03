using SupportWay.API.DTOs;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;
using ApiRequestItemDto = SupportWay.API.DTOs.RequestItemDto;

public class HelpRequestService : IHelpRequestService
{
    private readonly IHelpRequestsRepository _helpRepo;
    private readonly ILocationsRepository _locationRepo;

    public HelpRequestService(
        IHelpRequestsRepository helpRepo,
        ILocationsRepository locationRepo)
    {
        _helpRepo = helpRepo;
        _locationRepo = locationRepo;
    }

    public async Task<IEnumerable<HelpRequestDto>> GetFeedAsync(string currentUserId, int page, int size)
    {
        var requests = await _helpRepo.GetHelpRequestsByFollowedUsersAsync(currentUserId, page, size);
        var result = requests.Select(MapToDto).ToList();

        if (!result.Any())
        {
            var all = await _helpRepo.GetAllHelpRequestsAsync(page, size);
            result = all.Select(MapToDto).ToList();
        }

        return result;
    }

    public async Task<IEnumerable<HelpRequestDto>> GetUserHelpRequestsAsync(string userId, int page, int size)
    {
        var requests = await _helpRepo.GetHelpRequestsByUserAsync(userId, page, size);
        return requests.Select(MapToDto).ToList();
    }

    public async Task<HelpRequestDto?> GetHelpRequestByIdAsync(Guid id)
    {
        var request = await _helpRepo.GetHelpRequestByIdAsync(id);
        return request is null ? null : MapToDto(request);
    }

    public async Task<HelpRequestDetailsDto?> GetHelpRequestDetailsAsync(Guid id)
    {
        var r = await _helpRepo.GetHelpRequestByIdAsync(id);
        if (r is null) return null;

        var lat = r.Latitude ?? r.Location?.Latitude;
        var lng = r.Longitude ?? r.Location?.Longitude;

        var collected = r.CollectedAmount;
        var target = r.TargetAmount;
        var progress = target > 0
            ? (int)Math.Min(100, Math.Round(collected / target * 100))
            : 0;

        string? photoBase64 = null;
        if (r.User?.Profile?.Photo is { Length: > 0 } photo)
            photoBase64 = Convert.ToBase64String(photo);

        string? imageBase64 = null;
        if (r.Image is { Length: > 0 } img)
            imageBase64 = Convert.ToBase64String(img);

        return new HelpRequestDetailsDto
        {
            Id = r.Id,
            LocationId = r.LocationId,
            LocationName = r.Location?.DistrictName ?? string.Empty,
            LocationAddress = r.Location?.Address ?? string.Empty,
            Latitude = lat,
            Longitude = lng,
            Content = r.Content,
            ImageBase64 = imageBase64,
            CreatedAt = r.CreatedAt,
            UserId = r.UserId,
            UserName = r.User?.UserName ?? string.Empty,
            AuthorPhotoBase64 = photoBase64,
            LikesCount = r.Likes?.Count ?? 0,
            CommentsCount = r.Comments?.Count ?? 0,
            TargetAmount = target,
            CollectedAmount = collected,
            TotalPayments = r.Payments?.Sum(p => p.Amount) ?? 0,
            IsActive = r.IsActive,
            ProgressPercent = progress,
            RequestItems = r.RequestItems?.Select(ri => new RequestItemDetailsDto
            {
                Id = ri.Id,
                HelpRequestId = ri.HelpRequestId,
                Name = ri.Name,
                Quantity = ri.Quantity,
                UnitPrice = ri.UnitPrice,
                SupportTypeId = ri.SupportTypeId,
                SupportTypeName = ri.SupportType?.NameOfType ?? string.Empty
            }).ToList() ?? new List<RequestItemDetailsDto>()
        };
    }

    public async Task<Guid> CreateHelpRequestAsync(HelpRequestCreateDto dto, string userId)
    {
        Guid? resolvedLocationId = dto.LocationId;

        if (resolvedLocationId is null &&
            (dto.Latitude.HasValue || dto.Longitude.HasValue ||
             !string.IsNullOrWhiteSpace(dto.Address) ||
             !string.IsNullOrWhiteSpace(dto.DistrictName)))
        {
            var newLocation = new SupportWay.Data.Models.Location
            {
                LocationId = Guid.NewGuid(),
                DistrictName = dto.DistrictName ?? dto.Address ?? string.Empty,
                Address = dto.Address ?? string.Empty,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude
            };
            await _locationRepo.AddAsync(newLocation);
            resolvedLocationId = newLocation.LocationId;
        }

        var helpRequest = new HelpRequest
        {
            Id = Guid.NewGuid(),
            Content = dto.Content,
            Image = dto.Image,
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            LocationId = resolvedLocationId
        };

        await _helpRepo.AddHelpRequestAsync(helpRequest);
        return helpRequest.Id;
    }

    public async Task DeleteHelpRequestAsync(Guid id)
        => await _helpRepo.DeleteHelpRequestAsync(id);

    private static HelpRequestDto MapToDto(HelpRequest r) => new()
    {
        Id = r.Id,
        Content = r.Content,
        Image = r.Image,
        CreatedAt = r.CreatedAt,
        UserId = r.UserId,
        UserName = r.User?.UserName ?? string.Empty,
        LikesCount = r.Likes?.Count ?? 0,
        CommentsCount = r.Comments?.Count ?? 0,
        LocationId = r.LocationId,
        LocationName = r.Location?.DistrictName ?? string.Empty,
        LocationAddress = r.Location?.Address,
        Latitude = r.Latitude ?? r.Location?.Latitude,
        Longitude = r.Longitude ?? r.Location?.Longitude,
        TotalPayments = r.Payments?.Sum(p => p.Amount) ?? 0,
        TargetAmount = r.TargetAmount,
        CollectedAmount = r.CollectedAmount,
        IsActive = r.IsActive,
        RequestItems = r.RequestItems?.Select(MapItemToDto).ToList() ?? new List<ApiRequestItemDto>()
    };

    private static ApiRequestItemDto MapItemToDto(RequestItem item) => new ApiRequestItemDto
    {
        Id = item.Id,
        HelpRequestId = item.HelpRequestId,
        SupportTypeId = item.SupportTypeId,
        Name = item.Name,
        Quantity = item.Quantity,
        UnitPrice = item.UnitPrice,
        SupportTypeName = item.SupportType?.NameOfType ?? string.Empty
    };
}