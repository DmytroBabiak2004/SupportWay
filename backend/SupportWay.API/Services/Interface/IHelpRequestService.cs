public interface IHelpRequestService
{
    Task<IEnumerable<HelpRequestDto>> GetUserHelpRequestsAsync(string userId, int page, int size);
    Task<HelpRequestDto?> GetHelpRequestByIdAsync(int id);
    Task CreateHelpRequestAsync(HelpRequestCreateDto dto, string userId);
    Task DeleteHelpRequestAsync(int id);
}
