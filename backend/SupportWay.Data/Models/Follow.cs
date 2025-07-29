using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportWay.Data.Models
{
    public class Follow
    {
        public string FollowerId { get; set; }  
        public string FollowedId { get; set; } 
        public User Follower { get; set; }
        public User Followed { get; set; }
        public DateTime FollowedAt { get; set; }
    }

}
