namespace SupportWay.API.DTOs
{
    public record UpdateMessageRequest(string Content);
    public class MessageDto
    {
        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public string SenderId { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        // public bool IsEdited { get; set; } 
    }
}
