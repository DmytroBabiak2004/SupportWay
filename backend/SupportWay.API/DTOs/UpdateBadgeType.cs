using System;
using System.ComponentModel.DataAnnotations;

namespace SupportWay.API.DTOs.BadgeTypes
{
    public class UpdateBadgeTypeRequest
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    }
}