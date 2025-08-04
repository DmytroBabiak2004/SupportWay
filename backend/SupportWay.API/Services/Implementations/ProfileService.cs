using SupportWay.API.DTOs;
using SupportWay.API.Services.Interface;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IProfilesRepository _profilesRepository;
        public ProfileService(IProfilesRepository profilesRepository)
        {
            _profilesRepository = profilesRepository;
        }

        public async Task AddProfileAsync(string userId)
        {
            if (await _profilesRepository.ExistsAsync(userId))
                return;

            var profile = new Profile
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Description = string.Empty,
                Rating = 0
            };

            await _profilesRepository.AddAsync(profile);
        }

        public async Task<ProfileDto?> GetProfileAsync(string userId)
        {
            var profile = await _profilesRepository.GetByUserIdAsync(userId);
            if (profile == null) return null;

            return new ProfileDto
            {
                UserId = profile.UserId,
                Description = profile.Description,
                Rating = profile.Rating,
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
