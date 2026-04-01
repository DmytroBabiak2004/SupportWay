namespace SupportWay.Data.Models
{
    public class Chat
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime StartedAt { get; set; }
        public bool IsPrivate { get; set; } = true;

        public ICollection<UserChat> UserChats { get; set; } = new List<UserChat>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
