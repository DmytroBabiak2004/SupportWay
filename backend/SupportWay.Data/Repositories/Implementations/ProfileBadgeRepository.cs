using Microsoft.EntityFrameworkCore;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.Data.Repositories.Implementations
{
    public class ProfileBadgeRepository : IProfileBadgeRepository
    {
        private readonly SupportWayContext _context;

        public ProfileBadgeRepository(SupportWayContext context)
        {
            _context = context;
        }

        public async Task<ProfileBadge?> GetByProfileAndBadgeAsync(Guid profileId, Guid badgeId)
        {
            return await _context.ProfileBadges
                .Include(pb => pb.Badge)
                    .ThenInclude(b => b.BadgeType)
                .FirstOrDefaultAsync(pb => pb.ProfileId == profileId && pb.BadgeId == badgeId);
        }

        public async Task<bool> ExistsAsync(Guid profileId, Guid badgeId)
        {
            return await _context.ProfileBadges
                .AnyAsync(pb => pb.ProfileId == profileId && pb.BadgeId == badgeId);
        }

        public async Task AddAsync(ProfileBadge profileBadge)
        {
            await _context.ProfileBadges.AddAsync(profileBadge);
        }

        public void Delete(ProfileBadge profileBadge)
        {
            _context.ProfileBadges.Remove(profileBadge);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}