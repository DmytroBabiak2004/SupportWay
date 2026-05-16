using SupportWay.Data.Models;

namespace SupportWay.API.DTOs
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        public string? ImageBase64 { get; set; }
    }

    public class NotificationPagedResponse
    {
        public IEnumerable<NotificationDto> Items { get; set; } = [];
        public int UnreadCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class UnreadCountResponse
    {
        public int Count { get; set; }
    }
}