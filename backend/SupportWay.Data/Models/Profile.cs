
namespace SupportWay.Data.Models
{
    public class Profile
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public double Rating { get; set; }
        public byte[] Photo { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
