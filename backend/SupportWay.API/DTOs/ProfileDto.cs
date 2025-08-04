namespace SupportWay.API.DTOs
{
    public class ProfileDto
    {
        public string UserId { get; set; }
        public string Description { get; set; }
        public double Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? PhotoBase64 { get; set; }
    }

}
