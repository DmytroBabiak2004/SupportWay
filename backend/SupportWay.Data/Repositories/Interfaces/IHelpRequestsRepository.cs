using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SupportWay.Data.Models;
using SupportWay.Data.DTOs;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IHelpRequestsRepository
    {
        Task<IEnumerable<HelpRequest>> GetHelpRequestsByFollowedUsersAsync(string currentUserId, int pageNumber, int pageSize);
        Task<IEnumerable<HelpRequest>> GetHelpRequestsByUserAsync(string userId, int pageNumber, int pageSize);
        Task<IEnumerable<HelpRequest>> GetAllHelpRequestsAsync(int pageNumber, int pageSize);
        Task<HelpRequest> GetHelpRequestByIdAsync(Guid helpRequestId);
        Task AddHelpRequestAsync(HelpRequest helpRequest);
        Task UpdateHelpRequestAsync(HelpRequest helpRequest);
        Task DeleteHelpRequestAsync(Guid helpRequestId);
        Task<(IEnumerable<HelpRequest> Items, int Total)> GetForMapAsync(
    MapFilterParams filter,
    CancellationToken ct = default);
    }
}
