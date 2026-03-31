namespace SupportWay.API.DTOs
{
    public class FollowUserDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? FullName { get; set; }
        public string? PhotoBase64 { get; set; }
        public bool IsVerified { get; set; }
        public int? VerifiedAs { get; set; }
    }
}
