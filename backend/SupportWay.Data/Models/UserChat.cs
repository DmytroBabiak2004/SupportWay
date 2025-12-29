using System;

namespace SupportWay.Data.Models
{
    public class UserChat
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;

        public Guid ChatId { get; set; }
        public Chat Chat { get; set; } = null!;
    }
}
