using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportWay.Data.Models
{
    public class ProfileRating
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string RaterUserId { get; set; }
        public Guid RatedProfileId { get; set; }
        public int Value { get; set; }
        public DateTime RatedAt { get; set; }
        public User RaterUser { get; set; }
        public Profile RatedProfile { get; set; }
    }

}
