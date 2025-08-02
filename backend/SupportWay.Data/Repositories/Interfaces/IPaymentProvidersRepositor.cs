using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IPaymentProviderRepository
    {
        Task<IEnumerable<PaymentProvider>> GetAllAsync();
        Task<PaymentProvider> GetByIdAsync(int id);
        Task AddAsync(PaymentProvider provider);
        Task UpdateAsync(PaymentProvider provider);
        Task DeleteAsync(int id);
    }
}
