using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SupportWay.API.Services.Interface;
using SupportWay.API.Services.Interfaces;

namespace SupportWay.API.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly INotificationService _notificationService;

        public NotificationHub(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId))
                throw new HubException("Unauthorized");

            // Push current unread count immediately on connect so the
            // bell badge is accurate without a separate HTTP call.
            var count = await _notificationService.GetUnreadCountAsync(userId);
            await Clients.Caller.SendAsync("unreadCountUpdated", count);

            await base.OnConnectedAsync();
        }

        public async Task MarkAsRead(Guid notificationId)
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId))
                throw new HubException("Unauthorized");

            await _notificationService.MarkAsReadAsync(notificationId, userId);
        }

        public async Task MarkAllAsRead()
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId))
                throw new HubException("Unauthorized");

            await _notificationService.MarkAllAsReadAsync(userId);
        }
    }
}