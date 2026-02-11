using Microsoft.EntityFrameworkCore;
using SupportWay.API.DTOs;
using SupportWay.API.Services.Interface;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;
using SupportWay.Services.Interfaces; 

namespace SupportWay.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IProfilesRepository _profilesRepository;
        private readonly IProfileRatingRepository _profileRatingRepository;
        private readonly IFollowService _followService; 
        private readonly SupportWayContext _context;

        public ProfileService(
            IProfilesRepository profilesRepository,
            IProfileRatingRepository profileRatingRepository,
            IFollowService followService,
            SupportWayContext context)
        {
            _profilesRepository = profilesRepository;
            _profileRatingRepository = profileRatingRepository;
            _followService = followService;
            _context = context;
        }

        public async Task<ProfileDto?> GetProfileAsync(string userId)
        {
            var profile = await _profilesRepository.GetByUserIdAsync(userId);

            if (profile == null) return null;

            var rating = await _profileRatingRepository.GetAverageRatingAsync(profile.Id);
            var followersCount = await _followService.GetFollowersCountAsync(userId);
            var followingCount = await _followService.GetFollowingCountAsync(userId);

            return new ProfileDto
            {
                ProfileId = profile.Id,
                UserId = profile.UserId,
                Username = profile.User?.UserName ?? "Unknown",
                Name = profile.Name,
                FullName = profile.FullName,
                Description = profile.Description,
                CreatedAt = profile.CreatedAt,
                PhotoBase64 = profile.Photo != null ? Convert.ToBase64String(profile.Photo) : null,

                Rating = rating,
                FollowersCount = followersCount,
                FollowingCount = followingCount
            };
        }

        public async Task AddProfileAsync(string userId)
        {

            await AddProfileAsync(userId, null, null);
        }

        public async Task AddProfileAsync(string userId, string? name, string? fullName)
        {
            if (await _profilesRepository.ExistsAsync(userId))
                return;

            var defaultAvatar = await _context.DefaultAvatars.FirstOrDefaultAsync();

            var profile = new Profile
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Description = string.Empty,
                Photo = defaultAvatar?.Image,

                Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim(),
                FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim()
            };

            await _profilesRepository.AddAsync(profile);
        }
        public async Task<ProfileDto?> GetProfileByUsernameAsync(string username)
        {
            var profile = await _context.Profiles
                .Include(p => p.User) 
                .FirstOrDefaultAsync(p => p.User.UserName == username);

            if (profile == null) return null;

            var rating = await _profileRatingRepository.GetAverageRatingAsync(profile.Id);
            var followersCount = await _followService.GetFollowersCountAsync(profile.UserId);
            var followingCount = await _followService.GetFollowingCountAsync(profile.UserId);

            return new ProfileDto
            {
                ProfileId = profile.Id,
                UserId = profile.UserId,
                Username = profile.User?.UserName ?? "Unknown",
                Name = profile.Name,
                FullName = profile.FullName,
                Description = profile.Description,
                CreatedAt = profile.CreatedAt,
                PhotoBase64 = profile.Photo != null ? Convert.ToBase64String(profile.Photo) : null,
                Rating = rating,
                FollowersCount = followersCount,
                FollowingCount = followingCount
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
        public async Task UpdateNameAsync(string userId, string? name, string? fullName)
        {
            name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
            fullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim();

            await _profilesRepository.UpdateNameAsync(userId, name, fullName);
        }

    }
}