using System.ComponentModel.DataAnnotations;

namespace SupportWay.Data.Models
{
    public class Location
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [MaxLength(255)]
        public string Address { get; set; }
        [MaxLength(100)]
        public string DistrictName { get; set; }
        // GPS coordinates
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
