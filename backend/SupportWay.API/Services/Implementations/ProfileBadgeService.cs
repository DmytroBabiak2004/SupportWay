using SupportWay.API.Repositories.Interfaces;
using SupportWay.API.Services.Interfaces;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.API.Services.Implementations
{
    public class ProfileBadgeService : IProfileBadgeService
    {
        private readonly IProfileBadgeRepository _profileBadgeRepository;
        private readonly IBadgeRepository _badgeRepository;

        public ProfileBadgeService(
            IProfileBadgeRepository profileBadgeRepository,
            IBadgeRepository badgeRepository)
        {
            _profileBadgeRepository = profileBadgeRepository;
            _badgeRepository = badgeRepository;
        }

        public async Task RemoveBadgeFromProfileAsync(Guid profileId, Guid badgeId)
        {
            var profileBadge = await _profileBadgeRepository.GetByProfileAndBadgeAsync(profileId, badgeId);

            if (profileBadge == null)
                throw new KeyNotFoundException("Нагороду не знайдено у профілі.");

            _profileBadgeRepository.Delete(profileBadge);
            await _profileBadgeRepository.SaveChangesAsync();
        }

        public async Task AwardBadgeToProfileAsync(Guid profileId, Guid badgeId)
        {
            // Перевіряємо, що Badge існує (а разом із ним — і його BadgeType)
            var badge = await _badgeRepository.GetByIdAsync(badgeId);

            if (badge == null)
                throw new KeyNotFoundException("Нагороду не знайдено.");

            // Перевірка: badge.BadgeType вже завантажений через Include в GetByIdAsync
            // Тут можна додати перевірку конкретного типу, якщо бізнес-логіка вимагає
            var alreadyExists = await _profileBadgeRepository.ExistsAsync(profileId, badgeId);

            if (alreadyExists)
                throw new InvalidOperationException("Профіль вже має цю нагороду.");

            var profileBadge = new ProfileBadge
            {
                Id = Guid.NewGuid(),
                ProfileId = profileId,
                BadgeId = badgeId,
                AwardedAt = DateTime.UtcNow
            };

            await _profileBadgeRepository.AddAsync(profileBadge);
            await _profileBadgeRepository.SaveChangesAsync();
        }
    }
}