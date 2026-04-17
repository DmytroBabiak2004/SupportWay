namespace SupportWay.API.Services.Interfaces
{
    /// <summary>
    /// Автоматично перевіряє і видає нагороди профілю на основі метрик.
    /// Викликається після дій користувача (створення запиту, поста тощо).
    /// </summary>
    public interface IBadgeAwardService
    {
        /// <summary>
        /// Перевіряє нагороди типу "HelpRequest" після створення запиту на допомогу.
        /// Порівнює поточну кількість запитів користувача із Threshold кожного Badge.
        /// </summary>
        Task CheckAndAwardHelpRequestBadgesAsync(string userId);
    }
}
