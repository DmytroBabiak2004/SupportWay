using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportWay.Data.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public byte[]? Image { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public ICollection<PostLike> Likes { get; set; }
        public ICollection<PostComment> Comments { get; set; }

    }

}
