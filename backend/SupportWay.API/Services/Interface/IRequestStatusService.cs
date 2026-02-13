using SupportWay.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace SupportWay.Services.Interfaces
{
    public interface IRequestStatusService
    {
        Task<IEnumerable<RequestStatus>> GetAllAsync();
        Task<RequestStatus> GetByIdAsync(Guid id);
        Task AddAsync(RequestStatus status);
        Task UpdateAsync(RequestStatus status);
        Task DeleteAsync(Guid id);
    }
}