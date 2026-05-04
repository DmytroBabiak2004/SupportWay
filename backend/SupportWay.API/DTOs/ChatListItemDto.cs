namespace SupportWay.API.DTOs
{
    public class ChatListItemDto
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string? OtherUserId { get; set; }
        public string? OtherUserPhotoBase64 { get; set; }
        public bool OtherUserIsVerified { get; set; }
        public int? OtherUserVerifiedAs { get; set; }
        public string? LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
        public bool IsPrivate { get; set; }
    }
}
