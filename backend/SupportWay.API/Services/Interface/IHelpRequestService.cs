public interface IHelpRequestService
{
    Task<IEnumerable<HelpRequestDto>> GetUserHelpRequestsAsync(string userId, int page, int size);
    Task<HelpRequestDto?> GetHelpRequestByIdAsync(Guid id);
    Task CreateHelpRequestAsync(HelpRequestCreateDto dto, string userId);
    Task DeleteHelpRequestAsync(Guid id);
}
