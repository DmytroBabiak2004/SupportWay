using SupportWay.API.Repositories.Interfaces;
using SupportWay.API.Services.Interface;
using SupportWay.API.Services.Interfaces;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.API.Services.Implementations
{
    public class BadgeAwardService : IBadgeAwardService
    {
        private static readonly Dictionary<string, string> SupportTypeToBadgeTypeMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["Медична допомога"] = "Медична допомога",
                ["Гуманітарна допомога"] = "Гуманітарна допомога",
                ["Логістика"] = "Логістика",
                ["Дрони та БПЛА"] = "Дрони та БПЛА"
            };

        private readonly IRequestItemsRepository _requestItemsRepository;
        private readonly IBadgeRepository _badgeRepository;
        private readonly IProfileBadgeRepository _profileBadgeRepository;
        private readonly IProfilesRepository _profilesRepository;
        private readonly ILogger<BadgeAwardService> _logger;
        private readonly INotificationService _notificationService;

        public BadgeAwardService(
            IRequestItemsRepository requestItemsRepository,
            IBadgeRepository badgeRepository,
            IProfileBadgeRepository profileBadgeRepository,
            IProfilesRepository profilesRepository,
            ILogger<BadgeAwardService> logger,
            INotificationService notificationService) // ✅ БАГ 1 ВИПРАВЛЕНО: додано параметр
        {
            _requestItemsRepository = requestItemsRepository;
            _badgeRepository = badgeRepository;
            _profileBadgeRepository = profileBadgeRepository;
            _profilesRepository = profilesRepository;
            _logger = logger;
            _notificationService = notificationService; // ✅ тепер не null
        }

        public async Task CheckAndAwardRequestItemBadgesAsync(Guid requestItemId)
        {
            var requestItem = await _requestItemsRepository.GetByIdAsync(requestItemId);
            if (requestItem is null)
            {
                _logger.LogWarning("BadgeAwardService: RequestItem {RequestItemId} не знайдено.", requestItemId);
                return;
            }

            var userId = requestItem.HelpRequest?.UserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning(
                    "BadgeAwardService: для RequestItem {RequestItemId} не знайдено автора HelpRequest.",
                    requestItemId);
                return;
            }

            var supportTypeName = requestItem.SupportType?.NameOfType?.Trim();
            if (string.IsNullOrWhiteSpace(supportTypeName))
            {
                _logger.LogWarning(
                    "BadgeAwardService: для RequestItem {RequestItemId} не визначено SupportType.",
                    requestItemId);
                return;
            }

            if (!TryResolveBadgeTypeName(supportTypeName, out var badgeTypeName))
            {
                _logger.LogInformation(
                    "BadgeAwardService: для SupportType '{SupportType}' не налаштовано відповідний BadgeType.",
                    supportTypeName);
                return;
            }

            var profile = await EnsureProfileAsync(userId);

            var requestItemCount = await _requestItemsRepository.CountByUserIdAndSupportTypeAsync(
                userId,
                requestItem.SupportTypeId);

            var badges = await _badgeRepository.GetByTypeNameAsync(badgeTypeName);
            if (!badges.Any())
            {
                _logger.LogInformation(
                    "BadgeAwardService: для BadgeType '{BadgeType}' не знайдено жодного Badge.",
                    badgeTypeName);
                return;
            }

            var eligibleBadges = badges
                .Where(b => b.Threshold <= requestItemCount)
                .OrderBy(b => b.Threshold)
                .ToList();

            if (!eligibleBadges.Any())
            {
                _logger.LogDebug(
                    "BadgeAwardService: користувач {UserId} має {Count} RequestItem типу '{SupportType}', але ще не досяг порогу.",
                    userId,
                    requestItemCount,
                    supportTypeName);
                return;
            }

            // ✅ БАГ 2 ВИПРАВЛЕНО: збираємо нові нагороди ДО SaveChanges
            var newlyAwardedBadges = new List<Badge>();

            foreach (var badge in eligibleBadges)
            {
                var alreadyHasBadge = await _profileBadgeRepository.ExistsAsync(profile.Id, badge.Id);
                if (alreadyHasBadge)
                    continue;

                await _profileBadgeRepository.AddAsync(new ProfileBadge
                {
                    Id = Guid.NewGuid(),
                    ProfileId = profile.Id,
                    BadgeId = badge.Id,
                    AwardedAt = DateTime.UtcNow
                });

                newlyAwardedBadges.Add(badge); // запам'ятовуємо нові нагороди

                _logger.LogInformation(
                    "BadgeAwardService: нагороду '{BadgeName}' видано користувачу {UserId} для SupportType '{SupportType}'. Count={Count}, Threshold={Threshold}.",
                    badge.Name,
                    userId,
                    supportTypeName,
                    requestItemCount,
                    badge.Threshold);
            }

            if (newlyAwardedBadges.Count > 0)
            {
                await _profileBadgeRepository.SaveChangesAsync();

                // ✅ тепер ітеруємо по вже відомих нових нагородах — без повторної перевірки ExistsAsync
                foreach (var badge in newlyAwardedBadges)
                {
                    var imageBase64 = badge.Image is { Length: > 0 }
                        ? Convert.ToBase64String(badge.Image)
                        : null;

                    await _notificationService.CreateAndSendAsync(
                        userId: userId,
                        title: "Нову нагороду отримано!",
                        message: $"Ви отримали нагороду «{badge.Name}»",
                        type: NotificationType.BadgeAwarded,
                        relatedEntityId: badge.Id,
                        relatedEntityType: "badge",
                        imageBase64: imageBase64);
                }
            }
        }

        private static bool TryResolveBadgeTypeName(string supportTypeName, out string badgeTypeName)
        {
            if (SupportTypeToBadgeTypeMap.TryGetValue(supportTypeName.Trim(), out badgeTypeName!))
                return true;

            badgeTypeName = string.Empty;
            return false;
        }

        private async Task<Profile> EnsureProfileAsync(string userId)
        {
            var existingProfile = await _profilesRepository.GetByUserIdAsync(userId);
            if (existingProfile is not null)
                return existingProfile;

            var profile = new Profile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Description = string.Empty
            };

            await _profilesRepository.AddAsync(profile);

            _logger.LogInformation(
                "BadgeAwardService: для користувача {UserId} автоматично створено Profile {ProfileId}.",
                userId,
                profile.Id);

            return profile;
        }
    }
}