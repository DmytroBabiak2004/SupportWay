using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IPaymentStatusesRepository
    {
        Task<IEnumerable<PaymentStatus>> GetAllAsync();
        Task<PaymentStatus> GetByIdAsync(int id);
        Task AddAsync(PaymentStatus status);
        Task UpdateAsync(PaymentStatus status);
        Task DeleteAsync(int id);
    }
}
