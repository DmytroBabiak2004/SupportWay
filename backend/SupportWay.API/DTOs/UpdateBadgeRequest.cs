using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace SupportWay.API.DTOs
{
    public class UpdateBadgeRequest
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public decimal Threshold { get; set; }

        [Required]
        public Guid BadgeTypeId { get; set; }

        public IFormFile? Image { get; set; }
    }
}