namespace SupportWay.API.Services.Interfaces
{
    /// <summary>
    /// Перевіряє та видає нагороди після створення/оновлення RequestItem.
    /// Логіка прив'язана до SupportType RequestItem і BadgeType з відповідною назвою.
    /// </summary>
    public interface IBadgeAwardService
    {
        /// <summary>
        /// Завантажує RequestItem з пов'язаними даними, визначає відповідний BadgeType
        /// і видає всі нагороди, для яких користувач уже виконав поріг Threshold.
        /// </summary>
        Task CheckAndAwardRequestItemBadgesAsync(Guid requestItemId);
    }
}
