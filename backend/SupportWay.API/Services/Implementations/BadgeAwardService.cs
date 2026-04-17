using SupportWay.API.Repositories.Interfaces;
using SupportWay.API.Services.Interfaces;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.API.Services.Implementations
{
    /// <summary>
    /// Автоматично видає нагороди на основі метрик профілю.
    ///
    /// Логіка:
    ///   - Badge.BadgeType.Name — визначає тип метрики ("HelpRequest", "Post" тощо)
    ///   - Badge.Threshold       — мінімальна кількість дій для отримання нагороди
    ///
    /// Метод CheckAndAwardHelpRequestBadgesAsync:
    ///   1. Рахує кількість запитів на допомогу, створених користувачем.
    ///   2. Завантажує всі Badge типу "HelpRequest".
    ///   3. Фільтрує ті, де Threshold <= кількість запитів.
    ///   4. Видає лише ті, яких ще немає у профілі.
    /// </summary>
    public class BadgeAwardService : IBadgeAwardService
    {
        // Константа типу нагороди — має відповідати значенню BadgeType.Name у БД
        public const string HelpRequestBadgeType = "HelpRequest";

        private readonly IHelpRequestsRepository _helpRequestsRepository;
        private readonly IBadgeRepository _badgeRepository;
        private readonly IProfileBadgeRepository _profileBadgeRepository;
        private readonly IProfilesRepository _profilesRepository;
        private readonly ILogger<BadgeAwardService> _logger;

        public BadgeAwardService(
            IHelpRequestsRepository helpRequestsRepository,
            IBadgeRepository badgeRepository,
            IProfileBadgeRepository profileBadgeRepository,
            IProfilesRepository profilesRepository,
            ILogger<BadgeAwardService> logger)
        {
            _helpRequestsRepository = helpRequestsRepository;
            _badgeRepository = badgeRepository;
            _profileBadgeRepository = profileBadgeRepository;
            _profilesRepository = profilesRepository;
            _logger = logger;
        }

        public async Task CheckAndAwardHelpRequestBadgesAsync(string userId)
        {
            // 1. Знайти профіль користувача
            var profile = await _profilesRepository.GetByUserIdAsync(userId);
            if (profile is null)
            {
                _logger.LogWarning("BadgeAwardService: профіль для userId={UserId} не знайдено.", userId);
                return;
            }

            // 2. Порахувати кількість запитів на допомогу цього користувача
            var helpRequestCount = await _helpRequestsRepository.CountByUserIdAsync(userId);

            // 3. Завантажити всі нагороди типу "HelpRequest"
            var badges = await _badgeRepository.GetByTypeNameAsync(HelpRequestBadgeType);

            if (!badges.Any())
            {
                _logger.LogDebug("BadgeAwardService: нагород типу '{Type}' не знайдено.", HelpRequestBadgeType);
                return;
            }

            // 4. Видати нагороди, поріг яких досягнуто і яких ще немає у профілі
            foreach (var badge in badges.Where(b => (int)b.Threshold <= helpRequestCount))
            {
                var alreadyHas = await _profileBadgeRepository.ExistsAsync(profile.Id, badge.Id);
                if (alreadyHas) continue;

                await _profileBadgeRepository.AddAsync(new ProfileBadge
                {
                    Id = Guid.NewGuid(),
                    ProfileId = profile.Id,
                    BadgeId = badge.Id,
                    AwardedAt = DateTime.UtcNow
                });

                _logger.LogInformation(
                    "BadgeAwardService: нагороду '{BadgeName}' (поріг={Threshold}) видано профілю {ProfileId}.",
                    badge.Name, badge.Threshold, profile.Id);
            }

            await _profileBadgeRepository.SaveChangesAsync();
        }
    }
}
