using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IHelpRequestsRepository
    {
        Task<IEnumerable<HelpRequest>> GetHelpRequestsByFollowedUsersAsync(string currentUserId, int pageNumber, int pageSize);
        Task<IEnumerable<HelpRequest>> GetHelpRequestsByUserAsync(string userId, int pageNumber, int pageSize);
        Task<HelpRequest> GetHelpRequestByIdAsync(Guid helpRequestId);
        Task AddHelpRequestAsync(HelpRequest helpRequest);
        Task UpdateHelpRequestAsync(HelpRequest helpRequest);
        Task DeleteHelpRequestAsync(Guid helpRequestId);
    }
}
