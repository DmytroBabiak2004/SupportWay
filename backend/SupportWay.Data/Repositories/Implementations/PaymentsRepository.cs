using Microsoft.EntityFrameworkCore;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.Data.Repositories.Implementations
{
    public class PaymentsRepository : IPaymentsRepository
    {
        private readonly SupportWayContext _context;

        public PaymentsRepository(SupportWayContext context)
        {
            _context = context;
        }

        public async Task<Payment> GetByIdAsync(Guid id)
        {
            return await _context.Payments
                .Include(p => p.User)
                .Include(p => p.HelpRequest)
                .Include(p => p.PaymentProvider)
                .Include(p => p.PaymentStatus)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _context.Payments
                .Include(p => p.User)
                .Include(p => p.HelpRequest)
                .Include(p => p.PaymentProvider)
                .Include(p => p.PaymentStatus)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByUserIdAsync(string userId)
        {
            return await _context.Payments
                .Where(p => p.UserId == userId)
                .Include(p => p.PaymentProvider)
                .Include(p => p.PaymentStatus)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByStatusAsync(Guid statusId)
        {
            return await _context.Payments
                .Where(p => p.PaymentStatusId == statusId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByProviderAsync(Guid providerId)
        {
            return await _context.Payments
                .Where(p => p.PaymentProviderId == providerId)
                .ToListAsync();
        }

        public async Task AddAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
            }
        }
    }
}
