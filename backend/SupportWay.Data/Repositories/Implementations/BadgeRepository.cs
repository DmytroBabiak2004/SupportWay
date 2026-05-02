using Microsoft.EntityFrameworkCore;
using SupportWay.API.Repositories.Interfaces;
using SupportWay.Data.Context;
using SupportWay.Data.Models;

namespace SupportWay.API.Repositories
{
    public class BadgeRepository : IBadgeRepository
    {
        private readonly SupportWayContext _context;

        public BadgeRepository(SupportWayContext context)
        {
            _context = context;
        }

        public async Task<List<Badge>> GetAllAsync()
        {
            return await _context.Set<Badge>()
                .Include(x => x.BadgeType)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<Badge?> GetByIdAsync(Guid id)
        {
            return await _context.Set<Badge>()
                .Include(x => x.BadgeType)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Badge>> GetByProfileIdAsync(Guid profileId)
        {
            return await _context.ProfileBadges
                .Where(x => x.ProfileId == profileId)
                .Include(x => x.Badge)
                    .ThenInclude(x => x.BadgeType)
                .Select(x => x.Badge)
                .ToListAsync();
        }

        public async Task<List<Badge>> GetByTypeNameAsync(string typeName)
        {
            var normalizedTypeName = typeName.Trim().ToLower();

            return await _context.Set<Badge>()
                .Include(x => x.BadgeType)
                .Where(x => x.BadgeType.Name.ToLower() == normalizedTypeName)
                .OrderBy(x => x.Threshold)
                .ToListAsync();
        }

        public async Task<bool> BadgeTypeExistsAsync(Guid badgeTypeId)
        {
            return await _context.Set<BadgeType>()
                .AnyAsync(x => x.Id == badgeTypeId);
        }

        public async Task AddAsync(Badge badge)
        {
            await _context.Set<Badge>().AddAsync(badge);
        }

        public void Delete(Badge badge)
        {
            _context.Set<Badge>().Remove(badge);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
