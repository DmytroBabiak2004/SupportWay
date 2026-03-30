using System;
using System.ComponentModel.DataAnnotations;

namespace SupportWay.Data.DTOs
{
    public class LocationDto
    {
        public Guid? LocationId { get; set; }
        public string DistrictName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class CreateLocationDto
    {
        [MaxLength(100, ErrorMessage = "Назва не може перевищувати 100 символів")]
        public string? DistrictName { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}