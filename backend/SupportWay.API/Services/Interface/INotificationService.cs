using SupportWay.API.DTOs;
using SupportWay.Data.Models;

namespace SupportWay.API.Services.Interface
{
    public interface INotificationService
    {
        Task<NotificationPagedResponse> GetNotificationsAsync(
            string userId, int page, int pageSize, bool? unreadOnly = null);

        Task<int> GetUnreadCountAsync(string userId);

        Task MarkAsReadAsync(Guid id, string userId);

        Task MarkAllAsReadAsync(string userId);

        Task DeleteAsync(Guid id, string userId);

        Task CreateAndSendAsync(
            string userId,
            string title,
            string message,
            NotificationType type,
            Guid? relatedEntityId = null,
            string? relatedEntityType = null,
            string? imageBase64 = null);
    }
}