using System;
using System.ComponentModel.DataAnnotations;

namespace SupportWay.Data.Models
{
    public class ProfileBadge
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ProfileId { get; set; }
        public Profile Profile { get; set; }

        public Guid BadgeId { get; set; }
        public Badge Badge { get; set; }

        public DateTime AwardedAt { get; set; }
    }
}