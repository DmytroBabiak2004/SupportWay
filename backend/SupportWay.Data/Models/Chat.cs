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
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartedAt { get; set; }
        public ICollection<User> Users { get; set; }
        public ICollection<ChatMessage> Messages { get; set; }
    }
}
