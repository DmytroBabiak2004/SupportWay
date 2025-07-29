using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportWay.Data.Models
{
    public class PostLike
    {
        public int Id { get; set; }

        public int RequestId { get; set; }
        public HelpRequest HelpRequest { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public DateTime LikedAt { get; set; }
    }

}
