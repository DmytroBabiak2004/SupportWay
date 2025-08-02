using Microsoft.EntityFrameworkCore;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.Data.Repositories.Implementations
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly SupportWayContext _context;

        public ProfileRepository(SupportWayContext context)
        {
            _context = context;
        }

        public async Task<Profile?> GetByUserIdAsync(string userId)
        {
            return await _context.Profiles
                                 .Include(p => p.User)
                                 .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task AddAsync(Profile profile)
        {
            await _context.Profiles.AddAsync(profile);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(string userId)
        {
            return await _context.Profiles.AnyAsync(p => p.UserId == userId);
        }

        public async Task UpdateDescriptionAsync(string userId, string description)
        {
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile is null) return;

            profile.Description = description;
            _context.Profiles.Update(profile);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePhotoAsync(string userId, byte[] photo)
        {
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile is null) return;

            profile.Photo = photo;
            _context.Profiles.Update(profile);
            await _context.SaveChangesAsync();
        }
    }
}
