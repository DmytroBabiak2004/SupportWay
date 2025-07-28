using System.ComponentModel.DataAnnotations;

namespace SupportWay.Data.Models
{
    public class Location
    {
        public int Id { get; set; }
        [MaxLength(255)]
        public string Address { get; set; }
        [MaxLength(100)]
        public string DistrictName { get; set; }
        // GPS coordinates
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
