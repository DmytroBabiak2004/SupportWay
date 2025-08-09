using Microsoft.EntityFrameworkCore;
using SupportWay.API.DTOs;
using SupportWay.API.Services.Interface;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Implementations;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IProfilesRepository _profilesRepository;
        private readonly IProfileRatingRepository _profileRatingRepository;
        private readonly SupportWayContext _context;
        public ProfileService(IProfilesRepository profilesRepository, IProfileRatingRepository profileRatingRepository, SupportWayContext context)
        {
            _profilesRepository = profilesRepository;
            _profileRatingRepository = profileRatingRepository;
            _context = context;
        }

        public async Task AddProfileAsync(string userId)
        {
            if (await _profilesRepository.ExistsAsync(userId))
                return;
            var defaultAvatar = await _context.DefaultAvatars.FirstAsync();
            var profile = new Profile
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Description = string.Empty,
                Photo = defaultAvatar.Image
            };

            await _profilesRepository.AddAsync(profile);
        }

        public async Task<ProfileDto?> GetProfileAsync(string userId)
        {
            var profile = await _profilesRepository.GetByUserIdAsync(userId);
            if (profile == null) return null;

            var rating = await _profileRatingRepository.GetAverageRatingAsync(profile.Id);

            return new ProfileDto
            {
                UserId = profile.UserId,
                Description = profile.Description,
                Rating = rating,
                CreatedAt = profile.CreatedAt,
                PhotoBase64 = profile.Photo != null ? Convert.ToBase64String(profile.Photo) : null
            };
        }

        public async Task UpdateDescriptionAsync(string userId, string description)
        {
            await _profilesRepository.UpdateDescriptionAsync(userId, description);
        }

        public async Task UpdatePhotoAsync(string userId, byte[] photo)
        {
            await _profilesRepository.UpdatePhotoAsync(userId, photo);
        }
    }
}
