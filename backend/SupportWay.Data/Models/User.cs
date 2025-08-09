using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportWay.Data.Models
{
    public class User : IdentityUser
    {
        public Guid ProfileId { get; set; }
        public Profile Profile { get; set; }
        public ICollection<Payment> Payments { get; set; }
        public ICollection<Post> Posts { get; set; }
        public virtual ICollection<Follow> Followings { get; set; } 
        public virtual ICollection<Follow> Followers { get; set; }
    }
}