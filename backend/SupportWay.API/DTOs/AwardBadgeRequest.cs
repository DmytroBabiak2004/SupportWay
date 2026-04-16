using System.ComponentModel.DataAnnotations;

namespace SupportWay.API.DTOs
{
    public class AwardBadgeRequest
    {
        [Required]
        public Guid ProfileId { get; set; }

        [Required]
        public Guid BadgeId { get; set; }
    }
}