using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportWay.Data.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MessageText { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; }
    }

}
