using SupportWay.Data.Models;

namespace SupportWay.API.DTOs
{
    public record UpdateMessageRequest(string Content);

    public class SharePostRequestDto
    {
        public Guid ChatId { get; set; }
        public Guid PostId { get; set; }
        public string? Caption { get; set; }
    }

    public class ShareHelpRequestRequestDto
    {
        public Guid ChatId { get; set; }
        public Guid HelpRequestId { get; set; }
        public string? Caption { get; set; }
    }

    public class SharedPreviewDto
    {
        public Guid Id { get; set; }
        public string EntityType { get; set; } = string.Empty;

        public string AuthorUserName { get; set; } = string.Empty;

        public string? AuthorPhotoBase64 { get; set; }

        public bool AuthorIsVerified { get; set; }

        public int? AuthorVerifiedAs { get; set; }

        public string? Title { get; set; }

        public string Content { get; set; } = string.Empty;

        public string? ImageBase64 { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class MessageDto
    {
        public Guid Id { get; set; }

        public Guid ChatId { get; set; }

        public string SenderId { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }

        public bool IsRead { get; set; }

        public MessageType MessageType { get; set; }

        public Guid? SharedPostId { get; set; }

        public Guid? SharedHelpRequestId { get; set; }

        public SharedPreviewDto? SharedPreview { get; set; }
    }
}