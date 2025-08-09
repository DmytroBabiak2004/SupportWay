using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IRequestStatusesRepository
    {
        Task<IEnumerable<RequestStatus>> GetAllAsync();
        Task<RequestStatus> GetByIdAsync(Guid id);
        Task AddAsync(RequestStatus status);
        Task UpdateAsync(RequestStatus status);
        Task DeleteAsync(Guid id);
    }
}
