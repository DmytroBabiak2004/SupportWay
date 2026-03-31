namespace SupportWay.Data.Models
{
    public enum VerificationType
    {
        Volunteer = 1,
        Military  = 2,
        User      = 3
    }

    public enum VerificationStatus
    {
        Pending  = 0,
        Approved = 1,
        Rejected = 2
    }

    public class VerificationRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;

        public VerificationType VerificationType { get; set; }

        public VerificationStatus Status { get; set; } = VerificationStatus.Pending;

        public string? Notes { get; set; }           // applicant's explanation
        public string? AdminComment { get; set; }     // admin's decision comment

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DecidedAt { get; set; }
    }
}
