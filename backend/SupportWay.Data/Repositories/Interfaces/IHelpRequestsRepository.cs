using SupportWay.Data.DTOs;
using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IHelpRequestsRepository
    {
        Task<IEnumerable<HelpRequest>> GetHelpRequestsByFollowedUsersAsync(string currentUserId, int pageNumber, int pageSize);
        Task<IEnumerable<HelpRequest>> GetHelpRequestsByUserAsync(string userId, int pageNumber, int pageSize);
        Task<IEnumerable<HelpRequest>> GetAllHelpRequestsAsync(int pageNumber, int pageSize);
        Task<HelpRequest?> GetHelpRequestByIdAsync(Guid helpRequestId);
        Task AddHelpRequestAsync(HelpRequest helpRequest);
        Task UpdateHelpRequestAsync(HelpRequest helpRequest);
        Task DeleteHelpRequestAsync(Guid helpRequestId);
        Task<(IEnumerable<MapMarkerDto> Items, int Total)> GetMapMarkersAsync(
            MapFilterParams filter,
            CancellationToken ct = default);
        Task<int> CountByUserIdAsync(string userId);
    }
}