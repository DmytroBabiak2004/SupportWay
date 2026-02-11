using System.ComponentModel.DataAnnotations;

namespace SupportWay.API.Models
{
    public class RegisterRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public string? Role { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [MaxLength(80)]
        public string FullName { get; set; }
    }
}
