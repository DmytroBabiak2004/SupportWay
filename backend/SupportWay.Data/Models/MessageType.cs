using System.ComponentModel.DataAnnotations;

namespace SupportWay.Data.Models
{
    public class MessageType
    {
         [Key]
        public Guid Id { get; set; }
        [MaxLength(100)]
        public string NameOfType { get; set; }
        public ICollection<ChatMessage> Messages { get; set; }
    }
}