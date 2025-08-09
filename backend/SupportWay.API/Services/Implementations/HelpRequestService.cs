using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

public class HelpRequestService : IHelpRequestService
{
    private readonly IHelpRequestsRepository _helpRepo;
    private readonly ILocationRepository _locationRepo;
    private readonly IRequestStatusesRepository _statusRepo;

    public HelpRequestService(
        IHelpRequestsRepository helpRepo,
        ILocationRepository locationRepo,
        IRequestStatusesRepository statusRepo)
    {
        _helpRepo = helpRepo;
        _locationRepo = locationRepo;
        _statusRepo = statusRepo;
    }

    public async Task<IEnumerable<HelpRequestDto>> GetUserHelpRequestsAsync(string userId, int page, int size)
    {
        var requests = await _helpRepo.GetHelpRequestsByUserAsync(userId, page, size);

        return requests.Select(r => new HelpRequestDto
        {
            Id = r.Id,
            Content = r.Content,
            CreatedAt = r.CreatedAt,
            UserId = r.UserId,
            LocationName = r.Location?.DistrictName ?? "",
            StatusName = r.RequestStatus?.NameOfStatus ?? "",
            TotalPayments = r.Payments?.Sum(p => p.Amount) ?? 0
        });
    }

    public async Task<HelpRequestDto?> GetHelpRequestByIdAsync(Guid id)
    {
        var r = await _helpRepo.GetHelpRequestByIdAsync(id);
        if (r == null) return null;

        return new HelpRequestDto
        {
            Id = r.Id,
            Content = r.Content,
            CreatedAt = r.CreatedAt,
            UserId = r.UserId,
            LocationName = r.Location?.DistrictName ?? "",
            StatusName = r.RequestStatus?.NameOfStatus ?? "",
            TotalPayments = r.Payments?.Sum(p => p.Amount) ?? 0
        };
    }

    public async Task CreateHelpRequestAsync(HelpRequestCreateDto dto, string userId)
    {
        var location = await _locationRepo.GetByIdAsync(dto.LocationId);
        var status = await _statusRepo.GetByIdAsync(dto.RequestStatusId);

        if (location == null || status == null)
            throw new Exception("Invalid location or status");

        var helpRequest = new HelpRequest
        {
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            Location = location,
            RequestStatus = status
        };

        await _helpRepo.AddHelpRequestAsync(helpRequest);
    }

    public async Task DeleteHelpRequestAsync(Guid id)
    {
        await _helpRepo.DeleteHelpRequestAsync(id);
    }
}
