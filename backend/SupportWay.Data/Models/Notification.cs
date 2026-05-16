using SupportWay.Data.Models;

namespace SupportWay.Data.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;

        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        public Guid? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        public string? ImageBase64 { get; set; }
    }
}