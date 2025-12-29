using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportWay.Data.Models
{
    public class Chat
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime StartedAt { get; set; }

        public ICollection<UserChat> UserChats { get; set; } = new List<UserChat>();
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }

}
