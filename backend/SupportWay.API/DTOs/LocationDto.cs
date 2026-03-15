using System;
using System.ComponentModel.DataAnnotations;

namespace SupportWay.Data.DTOs
{
    public class LocationDto
    {
        public Guid? LocationId { get; set; }

        public string DistrictName { get; set; }
    }
    public class CreateLocationDto
    {
        [Required(ErrorMessage = "Назва району є обов'язковою")]
        [MaxLength(100, ErrorMessage = "Назва не може перевищувати 100 символів")]
        public string DistrictName { get; set; }
    }
}