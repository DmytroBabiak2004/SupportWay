namespace SupportWay.API.DTOs
{
    public class BadgeTypeResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class BadgeResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Threshold { get; set; }
        public BadgeTypeResponse BadgeType { get; set; }
        public string? ImageBase64 { get; set; }
    }
    public class ProfileBadgeResponse
    {
        public Guid BadgeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Threshold { get; set; }
        public string? ImageBase64 { get; set; }
        public BadgeTypeResponse BadgeType { get; set; }
        public DateTime? AwardedAt { get; set; }
    }

}