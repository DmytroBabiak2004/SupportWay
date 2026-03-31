using SupportWay.Data.Models;

namespace SupportWay.API.DTOs
{
    public class SubmitVerificationDto
    {
        public VerificationType VerificationType { get; set; }
        public string? Notes { get; set; }
    }

    public class VerificationRequestDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? PhotoBase64 { get; set; }
        public VerificationType VerificationType { get; set; }
        public VerificationStatus Status { get; set; }
        public string? Notes { get; set; }
        public string? AdminComment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DecidedAt { get; set; }
    }

    public class DecideVerificationDto
    {
        public bool Approved { get; set; }
        public string? AdminComment { get; set; }
    }
}
