using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportWay.Data.Models
{
    public class ProfileRating
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string RaterUserId { get; set; }

        [ForeignKey(nameof(RaterUserId))]
        public User RaterUser { get; set; }
        public Guid RatedProfileId { get; set; }

        [ForeignKey(nameof(RatedProfileId))]
        public Profile RatedProfile { get; set; }

        public int Value { get; set; }
        public DateTime RatedAt { get; set; } = DateTime.UtcNow;
    }
}