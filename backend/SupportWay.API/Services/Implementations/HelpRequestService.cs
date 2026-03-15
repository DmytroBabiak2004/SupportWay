using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;
using SupportWay.Data.DTOs; // Не забудьте цей using
using System.Linq;

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

    // 1. Отримання списку для юзера
    public async Task<IEnumerable<HelpRequestDto>> GetUserHelpRequestsAsync(string userId, int page, int size)
    {
        var requests = await _helpRepo.GetHelpRequestsByUserAsync(userId, page, size);

        // Мапимо дані, включаючи поля від Post
        return requests.Select(r => MapToDto(r));
    }

    // 2. Отримання одного запиту
    public async Task<HelpRequestDto?> GetHelpRequestByIdAsync(Guid id)
    {
        var r = await _helpRepo.GetHelpRequestByIdAsync(id);
        if (r == null) return null;

        return MapToDto(r);
    }
    public async Task CreateHelpRequestAsync(HelpRequestCreateDto dto, string userId)
    {
        var location = await _locationRepo.GetByIdAsync(dto.LocationId);

        if (location == null)
            throw new ArgumentException("Invalid LocationId");

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

    // --- Private Helper для мапінгу (щоб не дублювати код) ---
    private HelpRequestDto MapToDto(HelpRequest r)
    {
        return new HelpRequestDto
        {
            Id = r.Id,
            // Поля Post
            Content = r.Content,
            Image = r.Image,
            CreatedAt = r.CreatedAt,
            UserId = r.UserId,
            UserName = r.User?.UserName ?? "Unknown", // Припускаємо, що у User є UserName

            // Статистика Post (якщо колекції null, повертаємо 0)
            LikesCount = r.Likes?.Count ?? 0,
            CommentsCount = r.Comments?.Count ?? 0,

            // Поля HelpRequest
            LocationId = r.LocationId,
            LocationName = r.Location?.DistrictName ?? "",
            TotalPayments = r.Payments?.Sum(p => p.Amount) ?? 0
        };
    }
}