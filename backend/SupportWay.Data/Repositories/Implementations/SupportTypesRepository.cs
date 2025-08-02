using Microsoft.EntityFrameworkCore;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;


namespace SupportWay.Data.Repositories.Implementations
{
    public class SupportTypesRepository : ISupportTypesRepository
    {
        private readonly SupportWayContext _context;

        public SupportTypesRepository(SupportWayContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SupportType>> GetAllAsync() =>
            await _context.SupportTypes.ToListAsync();

        public async Task<SupportType> GetByIdAsync(int id) =>
            await _context.SupportTypes.FindAsync(id);

        public async Task AddAsync(SupportType type)
        {
            _context.SupportTypes.Add(type);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SupportType type)
        {
            _context.SupportTypes.Update(type);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var type = await _context.SupportTypes.FindAsync(id);
            if (type != null)
            {
                _context.SupportTypes.Remove(type);
                await _context.SaveChangesAsync();
            }
        }
    }
}
