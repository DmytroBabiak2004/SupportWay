using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IPaymentsRepository
    {
        Task<Payment> GetByIdAsync(int id);
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<IEnumerable<Payment>> GetByUserIdAsync(string userId);
        Task AddAsync(Payment payment);
        Task UpdateAsync(Payment payment);
        Task DeleteAsync(int id);

        Task<IEnumerable<Payment>> GetByStatusAsync(int statusId);
        Task<IEnumerable<Payment>> GetByProviderAsync(int providerId);
    }
}
