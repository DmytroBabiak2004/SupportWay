using SupportWay.API.DTOs;
using SupportWay.API.Helpers;
using SupportWay.API.Repositories.Interfaces;
using SupportWay.API.Services.Interfaces;
using SupportWay.Data.Models;

namespace SupportWay.API.Services
{
    public class BadgeService : IBadgeService
    {
        private readonly IBadgeRepository _badgeRepository;

        public BadgeService(IBadgeRepository badgeRepository)
        {
            _badgeRepository = badgeRepository;
        }

        public async Task<List<BadgeResponse>> GetAllAsync()
        {
            var badges = await _badgeRepository.GetAllAsync();

            return badges.Select(x => new BadgeResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Threshold = x.Threshold,
                BadgeType = new BadgeTypeResponse
                {
                    Id = x.BadgeTypeId,
                    Name = x.BadgeType?.Name ?? string.Empty
                },
                ImageBase64 = x.Image != null && x.Image.Length > 0
                    ? Convert.ToBase64String(x.Image)
                    : null
            }).ToList();
        }
        public async Task<List<BadgeResponse>> GetByProfileIdAsync(Guid profileId)
        {
            var badges = await _badgeRepository.GetByProfileIdAsync(profileId);

            return badges.Select(x => new BadgeResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Threshold = x.Threshold,
                BadgeType = new BadgeTypeResponse
                {
                    Id = x.BadgeTypeId,
                    Name = x.BadgeType.Name
                },
                ImageBase64 = x.Image != null && x.Image.Length > 0
                    ? Convert.ToBase64String(x.Image)
                    : null
            }).ToList();
        }
        public async Task<BadgeResponse?> GetByIdAsync(Guid id)
        {
            var badge = await _badgeRepository.GetByIdAsync(id);

            if (badge == null)
                return null;

            return new BadgeResponse
            {
                Id = badge.Id,
                Name = badge.Name,
                Description = badge.Description,
                Threshold = badge.Threshold,
                BadgeType = new BadgeTypeResponse
                {
                    Id = badge.BadgeTypeId,
                    Name = badge.BadgeType?.Name ?? string.Empty
                },
                ImageBase64 = badge.Image != null && badge.Image.Length > 0
                    ? Convert.ToBase64String(badge.Image)
                    : null
            };
        }

        public async Task<Guid> CreateBadgeAsync(CreateBadgeRequest request)
        {
            var badgeTypeExists = await _badgeRepository.BadgeTypeExistsAsync(request.BadgeTypeId);

            if (!badgeTypeExists)
                throw new Exception("Тип нагороди не знайдено.");

            var imageBytes = await FileHelper.ConvertToBytesAsync(request.Image);

            var badge = new Badge
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Threshold = request.Threshold,
                BadgeTypeId = request.BadgeTypeId,
                Image = imageBytes
            };

            await _badgeRepository.AddAsync(badge);
            await _badgeRepository.SaveChangesAsync();

            return badge.Id;
        }

        public async Task UpdateBadgeAsync(UpdateBadgeRequest request)
        {
            var badge = await _badgeRepository.GetByIdAsync(request.Id);

            if (badge == null)
                throw new Exception("Нагороду не знайдено.");

            var badgeTypeExists = await _badgeRepository.BadgeTypeExistsAsync(request.BadgeTypeId);

            if (!badgeTypeExists)
                throw new Exception("Тип нагороди не знайдено.");

            badge.Name = request.Name;
            badge.Description = request.Description;
            badge.Threshold = request.Threshold;
            badge.BadgeTypeId = request.BadgeTypeId;

            if (request.Image != null)
            {
                badge.Image = await FileHelper.ConvertToBytesAsync(request.Image);
            }

            await _badgeRepository.SaveChangesAsync();
        }

        public async Task DeleteBadgeAsync(Guid id)
        {
            var badge = await _badgeRepository.GetByIdAsync(id);

            if (badge == null)
                throw new Exception("Нагороду не знайдено.");

            _badgeRepository.Delete(badge);
            await _badgeRepository.SaveChangesAsync();
        }
    }
}