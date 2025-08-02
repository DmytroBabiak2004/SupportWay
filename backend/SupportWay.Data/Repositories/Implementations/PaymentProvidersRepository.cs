using Microsoft.EntityFrameworkCore;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.Data.Repositories.Implementations
{
    public class PaymentProviderRepository : IPaymentProviderRepository
    {
        private readonly SupportWayContext _context;

        public PaymentProviderRepository(SupportWayContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PaymentProvider>> GetAllAsync()
        {
            return await _context.PaymentProviders.ToListAsync();
        }

        public async Task<PaymentProvider> GetByIdAsync(int id)
        {
            return await _context.PaymentProviders.FindAsync(id);
        }

        public async Task AddAsync(PaymentProvider provider)
        {
            _context.PaymentProviders.Add(provider);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PaymentProvider provider)
        {
            _context.PaymentProviders.Update(provider);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var provider = await _context.PaymentProviders.FindAsync(id);
            if (provider != null)
            {
                _context.PaymentProviders.Remove(provider);
                await _context.SaveChangesAsync();
            }
        }
    }
}
