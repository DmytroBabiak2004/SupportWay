using System.ComponentModel.DataAnnotations;

namespace SupportWay.API.DTOs.BadgeTypes
{
    public class CreateBadgeTypeRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    }
}