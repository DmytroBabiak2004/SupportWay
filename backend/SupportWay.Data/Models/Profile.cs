namespace SupportWay.Data.Models
{
    public class Profile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Name { get; set; }
        public string? FullName { get; set; }
        public string Description { get; set; } = string.Empty;
        public byte[]? Photo { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;

        // Verification
        public bool IsVerified { get; set; } = false;
        public VerificationType? VerifiedAs { get; set; }

        public ICollection<ProfileRating> ProfileRatings { get; set; } = new List<ProfileRating>();
        public ICollection<ProfileBadge> ProfileBadges { get; set; } = new List<ProfileBadge>();
        public ICollection<VerificationRequest> VerificationRequests { get; set; } = new List<VerificationRequest>();
    }
}
