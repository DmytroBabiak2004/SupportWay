namespace SupportWay.API.DTOs
{
    public class ProfileDto
    {
        public Guid ProfileId { get; set; }
        public string? Name { get; set; }
        public string? FullName { get; set; }
        public string UserId { get; set; }  
        public string Username { get; set; } 
        public string Description { get; set; }
        public string? PhotoBase64 { get; set; }
        public DateTime CreatedAt { get; set; }

        public double? Rating { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public bool IsOwnProfile { get; set; }
    }
    public class UpdateProfileNameDto
    {
        public string? Name { get; set; }
        public string? FullName { get; set; }
    }
}