using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SupportWay.Data.DTOs
{
    public class CreateHelpRequestRequest
    {
        [Required(ErrorMessage = "Заголовок обов'язковий")]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Опис обов'язковий")]
        public string Content { get; set; }

        public IFormFile? Image { get; set; }

        // Option A: існуюча локація
        public Guid? LocationId { get; set; }

        // Option B: нова локація (координати або пошук)
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Address { get; set; }
        public string? DistrictName { get; set; }
    }
}