using Microsoft.EntityFrameworkCore;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.Data.Repositories.Implementations
{
    public class PaymentStatusesRepository : IPaymentStatusesRepository
    {
        private readonly SupportWayContext _context;

        public PaymentStatusesRepository(SupportWayContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PaymentStatus>> GetAllAsync()
        {
            return await _context.PaymentStatuses.ToListAsync();
        }

        public async Task<PaymentStatus> GetByIdAsync(int id)
        {
            return await _context.PaymentStatuses.FindAsync(id);
        }

        public async Task AddAsync(PaymentStatus status)
        {
            _context.PaymentStatuses.Add(status);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PaymentStatus status)
        {
            _context.PaymentStatuses.Update(status);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var status = await _context.PaymentStatuses.FindAsync(id);
            if (status != null)
            {
                _context.PaymentStatuses.Remove(status);
                await _context.SaveChangesAsync();
            }
        }
    }
}
