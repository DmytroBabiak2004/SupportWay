using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IPaymentsRepository
    {
        Task<Payment> GetByIdAsync(Guid id);
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<IEnumerable<Payment>> GetByUserIdAsync(string userId);
        Task AddAsync(Payment payment);
        Task UpdateAsync(Payment payment);
        Task DeleteAsync(Guid id);

        Task<IEnumerable<Payment>> GetByStatusAsync(Guid statusId);
        Task<IEnumerable<Payment>> GetByProviderAsync(Guid providerId);
    }
}
