
namespace SupportWay.Data.Models
{
    public class Profile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Description { get; set; }
        public byte[] Photo { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public ICollection<ProfileRating> ProfileRatings { get; set; }
    }
}
