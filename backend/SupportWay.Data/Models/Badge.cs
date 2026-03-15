using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SupportWay.Data.Models
{
    public class Badge
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public byte[] Image { get; set; }
        public decimal Threshold { get; set; }

        public Guid BadgeTypeId { get; set; }
        public BadgeType BadgeType { get; set; }
        public ICollection<ProfileBadge> ProfileBadges { get; set; } = new List<ProfileBadge>();
    }
}