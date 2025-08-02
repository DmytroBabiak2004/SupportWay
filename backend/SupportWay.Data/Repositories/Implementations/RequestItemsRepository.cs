using Microsoft.EntityFrameworkCore;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.Data.Repositories.Implementations
{
    public class RequestItemsRepository : IRequestItemsRepository
    {
        private readonly SupportWayContext _context;

        public RequestItemsRepository(SupportWayContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RequestItem>> GetByHelpRequestIdAsync(int helpRequestId) =>
            await _context.RequestItems
                .Where(i => i.HelpRequestId == helpRequestId)
                .Include(i => i.SupportType)
                .ToListAsync();

        public async Task<RequestItem> GetByIdAsync(int id) =>
            await _context.RequestItems
                .Include(i => i.SupportType)
                .Include(i => i.HelpRequest)
                .FirstOrDefaultAsync(i => i.Id == id);

        public async Task AddAsync(RequestItem item)
        {
            _context.RequestItems.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RequestItem item)
        {
            _context.RequestItems.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var item = await _context.RequestItems.FindAsync(id);
            if (item != null)
            {
                _context.RequestItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}
