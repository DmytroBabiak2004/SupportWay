using SupportWay.API.DTOs;
using SupportWay.API.Services.Interfaces;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;
using ApiRequestItemDto = SupportWay.API.DTOs.RequestItemDto;

public class HelpRequestService : IHelpRequestService
{
    private readonly IHelpRequestsRepository _helpRepo;
    private readonly ILocationsRepository _locationRepo;
    private readonly IPostLikesRepoository _likesRepo;

    public HelpRequestService(
        IHelpRequestsRepository helpRepo,
        ILocationsRepository locationRepo,
        IPostLikesRepoository likesRepo)
    {
        _helpRepo = helpRepo;
        _locationRepo = locationRepo;
        _likesRepo = likesRepo;
    }

    public async Task<IEnumerable<HelpRequestDto>> GetFeedAsync(string currentUserId, int page, int size)
    {
        var requests = await _helpRepo.GetHelpRequestsByFollowedUsersAsync(currentUserId, page, size);
        var result = await MapToDtosAsync(requests, currentUserId);

        if (!result.Any())
        {
            var all = await _helpRepo.GetAllHelpRequestsAsync(page, size);
            result = await MapToDtosAsync(all, currentUserId);
        }

        return result;
    }

    public async Task<IEnumerable<HelpRequestDto>> GetUserHelpRequestsAsync(string userId, int page, int size)
    {
        var requests = await _helpRepo.GetHelpRequestsByUserAsync(userId, page, size);
        return await MapToDtosAsync(requests, userId);
    }

    public async Task<HelpRequestDto?> GetHelpRequestByIdAsync(Guid id, string? currentUserId = null)
    {
        var request = await _helpRepo.GetHelpRequestByIdAsync(id);
        return request is null ? null : await MapToDtoAsync(request, currentUserId);
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
            AuthorUserName = r.User?.UserName,
            AuthorFullName = r.User?.Profile?.FullName,
            AuthorPhotoBase64 = photoBase64,
            AuthorIsVerified = r.User?.Profile?.IsVerified ?? false,
            AuthorVerifiedAs = r.User?.Profile?.VerifiedAs.HasValue == true ? (int?)r.User.Profile.VerifiedAs.Value : null,
            LikesCount = r.Likes?.Select(l => l.UserId).Distinct().Count() ?? 0,
            CommentsCount = r.Comments?.Count ?? 0,
            IsLikedByCurrentUser = false,
            TargetAmount = target,
            CollectedAmount = collected,
            TotalPayments = r.Payments?.Sum(p => p.Amount) ?? 0,
            IsActive = r.IsActive,
            ProgressPercent = progress,
            PreferredDonationMethod = r.PreferredDonationMethod,
            DonationRecipientName = r.DonationRecipientName,
            DonationRecipientCardNumber = r.DonationRecipientCardNumber,
            DonationRecipientIban = r.DonationRecipientIban,
            DonationPaymentLink = r.DonationPaymentLink,
            DonationNotes = r.DonationNotes,
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

        var preferredMethod = string.IsNullOrWhiteSpace(dto.PreferredDonationMethod)
            ? ResolvePreferredDonationMethod(dto)
            : dto.PreferredDonationMethod!.Trim();

        var helpRequest = new HelpRequest
        {
            Id = Guid.NewGuid(),
            Content = dto.Content,
            Image = dto.Image,
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            LocationId = resolvedLocationId,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            PreferredDonationMethod = preferredMethod,
            DonationRecipientName = Normalize(dto.DonationRecipientName),
            DonationRecipientCardNumber = NormalizeCard(dto.DonationRecipientCardNumber),
            DonationRecipientIban = Normalize(dto.DonationRecipientIban)?.ToUpperInvariant(),
            DonationPaymentLink = Normalize(dto.DonationPaymentLink),
            DonationNotes = Normalize(dto.DonationNotes)
        };

        await _helpRepo.AddHelpRequestAsync(helpRequest);
        return helpRequest.Id;
    }

    public async Task DeleteHelpRequestAsync(Guid id)
        => await _helpRepo.DeleteHelpRequestAsync(id);

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeCard(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var digits = new string(value.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(digits) ? null : digits;
    }

    private static string? ResolvePreferredDonationMethod(HelpRequestCreateDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.DonationPaymentLink)) return "payment_link";
        if (!string.IsNullOrWhiteSpace(dto.DonationRecipientIban)) return "iban";
        if (!string.IsNullOrWhiteSpace(dto.DonationRecipientCardNumber)) return "bank_card";
        return "bank_card";
    }

    private async Task<List<HelpRequestDto>> MapToDtosAsync(IEnumerable<HelpRequest> requests, string currentUserId)
    {
        var result = new List<HelpRequestDto>();

        foreach (var request in requests)
        {
            result.Add(await MapToDtoAsync(request, currentUserId));
        }

        return result;
    }

    private async Task<HelpRequestDto> MapToDtoAsync(HelpRequest r, string? currentUserId)
    {
        var photoBase64 = r.User?.Profile?.Photo is { Length: > 0 } photo
            ? Convert.ToBase64String(photo)
            : null;

        var likesCount = r.Likes?.Select(l => l.UserId).Distinct().Count()
            ?? await _likesRepo.GetLikesCountAsync(r.Id);

        var isLiked = !string.IsNullOrWhiteSpace(currentUserId) &&
                      await _likesRepo.HasUserLikedPostAsync(r.Id, currentUserId);

        return new HelpRequestDto
        {
            Id = r.Id,
            Title = string.Empty,
            Content = r.Content,
            Image = r.Image,
            CreatedAt = r.CreatedAt,
            UserId = r.UserId,
            UserName = r.User?.UserName ?? string.Empty,
            AuthorUserName = r.User?.UserName,
            AuthorFullName = r.User?.Profile?.FullName,
            AuthorPhotoBase64 = photoBase64,
            AuthorIsVerified = r.User?.Profile?.IsVerified ?? false,
            AuthorVerifiedAs = r.User?.Profile?.VerifiedAs.HasValue == true ? (int?)r.User.Profile.VerifiedAs.Value : null,
            LikesCount = likesCount,
            CommentsCount = r.Comments?.Count ?? 0,
            IsLikedByCurrentUser = isLiked,
            LocationId = r.LocationId,
            LocationName = r.Location?.DistrictName ?? string.Empty,
            LocationAddress = r.Location?.Address,
            Latitude = r.Latitude ?? r.Location?.Latitude,
            Longitude = r.Longitude ?? r.Location?.Longitude,
            TotalPayments = r.Payments?.Sum(p => p.Amount) ?? 0,
            TargetAmount = r.TargetAmount,
            CollectedAmount = r.CollectedAmount,
            IsActive = r.IsActive,
            PreferredDonationMethod = r.PreferredDonationMethod,
            DonationRecipientName = r.DonationRecipientName,
            DonationRecipientCardNumber = r.DonationRecipientCardNumber,
            DonationRecipientIban = r.DonationRecipientIban,
            DonationPaymentLink = r.DonationPaymentLink,
            DonationNotes = r.DonationNotes,
            RequestItems = r.RequestItems?.Select(MapItemToDto).ToList() ?? new List<ApiRequestItemDto>()
        };
    }

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
