using System.ComponentModel.DataAnnotations;

namespace SupportWay.Data.Models
{
    public class MessageStatus
    {
        [Key]
        public Guid Id { get; set; }
        [MaxLength(100)]
        public string NameOfStatus { get; set; }
        public ChatMessage Message { get; set; }
    }
}
