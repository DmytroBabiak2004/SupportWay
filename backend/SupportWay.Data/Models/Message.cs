namespace SupportWay.Data.Models
{
    public enum MessageType
    {
        Text = 0,
        SharedPost = 1,
        SharedHelpRequest = 2
    }

    public class Message
    {
        public Guid Id { get; set; }

        public string Content { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        public DateTime SentAt { get; set; }

        public string SenderId { get; set; } = string.Empty;

        public Guid ChatId { get; set; }

        public Chat Chat { get; set; } = null!;

        public MessageType MessageType { get; set; } = MessageType.Text;

        public Guid? SharedPostId { get; set; }

        public Guid? SharedHelpRequestId { get; set; }
    }
}