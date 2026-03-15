using Microsoft.EntityFrameworkCore;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.Data.Repositories
{
    public class BadgeTypeRepository : IBadgeTypeRepository
    {
        private readonly SupportWayContext _context;

        public BadgeTypeRepository(SupportWayContext context)
        {
            _context = context;
        }

        public async Task<List<BadgeType>> GetAllAsync()
        {
            return await _context.Set<BadgeType>()
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<BadgeType?> GetByIdAsync(Guid id)
        {
            return await _context.Set<BadgeType>()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<BadgeType?> GetByNameAsync(string name)
        {
            return await _context.Set<BadgeType>()
                .FirstOrDefaultAsync(x => x.Name == name);
        }

        public async Task AddAsync(BadgeType badgeType)
        {
            await _context.Set<BadgeType>().AddAsync(badgeType);
        }

        public void Update(BadgeType badgeType)
        {
            _context.Set<BadgeType>().Update(badgeType);
        }

        public void Delete(BadgeType badgeType)
        {
            _context.Set<BadgeType>().Remove(badgeType);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}