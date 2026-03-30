using SupportWay.Data.DTOs;

public interface IHelpRequestService
{
    Task<IEnumerable<HelpRequestDto>> GetUserHelpRequestsAsync(string userId, int page, int size);
    Task<IEnumerable<HelpRequestDto>> GetFeedAsync(string currentUserId, int page, int size);
    Task<HelpRequestDto?> GetHelpRequestByIdAsync(Guid id);
    Task<Guid> CreateHelpRequestAsync(HelpRequestCreateDto dto, string userId);
    Task DeleteHelpRequestAsync(Guid id);
}
