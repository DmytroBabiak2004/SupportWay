using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using SupportWay.API.DTOs;
using SupportWay.API.Hubs;
using SupportWay.API.Services.Interface;
using SupportWay.API.Services.Interfaces;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.API.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IMapper _mapper;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository notificationRepository,
            IHubContext<NotificationHub> hubContext,
            IMapper mapper,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _hubContext = hubContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<NotificationPagedResponse> GetNotificationsAsync(
            string userId, int page, int pageSize, bool? unreadOnly = null)
        {
            var items = await _notificationRepository
                .GetByUserIdAsync(userId, page, pageSize, unreadOnly);
            var unread = await _notificationRepository.GetUnreadCountAsync(userId);

            return new NotificationPagedResponse
            {
                Items = _mapper.Map<IEnumerable<NotificationDto>>(items),
                UnreadCount = unread,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _notificationRepository.GetUnreadCountAsync(userId);
        }

        public async Task MarkAsReadAsync(Guid id, string userId)
        {
            await _notificationRepository.MarkAsReadAsync(id, userId);
            await _notificationRepository.SaveChangesAsync();

            var unread = await _notificationRepository.GetUnreadCountAsync(userId);
            await _hubContext.Clients.User(userId)
                .SendAsync("unreadCountUpdated", unread);
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            await _notificationRepository.MarkAllAsReadAsync(userId);
            // ExecuteUpdateAsync saves immediately — no SaveChangesAsync needed here

            await _hubContext.Clients.User(userId)
                .SendAsync("unreadCountUpdated", 0);
        }

        public async Task DeleteAsync(Guid id, string userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);

            if (notification is null || notification.UserId != userId)
                throw new KeyNotFoundException("Notification not found.");

            _notificationRepository.Delete(notification);
            await _notificationRepository.SaveChangesAsync();
        }

        public async Task CreateAndSendAsync(
            string userId,
            string title,
            string message,
            NotificationType type,
            Guid? relatedEntityId = null,
            string? relatedEntityType = null,
            string? imageBase64 = null)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                RelatedEntityId = relatedEntityId,
                RelatedEntityType = relatedEntityType,
                ImageBase64 = imageBase64
            };

            await _notificationRepository.AddAsync(notification);
            await _notificationRepository.SaveChangesAsync();

            var dto = _mapper.Map<NotificationDto>(notification);

            try
            {
                await _hubContext.Clients.User(userId)
                    .SendAsync("receiveNotification", dto);

                var unread = await _notificationRepository.GetUnreadCountAsync(userId);
                await _hubContext.Clients.User(userId)
                    .SendAsync("unreadCountUpdated", unread);
            }
            catch (Exception ex)
            {
                // Don't fail the whole operation if SignalR delivery fails —
                // the notification is already persisted and will appear on next load.
                _logger.LogWarning(ex,
                    "SignalR delivery failed for notification {NotificationId} to user {UserId}",
                    notification.Id, userId);
            }
        }
    }
}