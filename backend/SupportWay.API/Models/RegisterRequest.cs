using System.ComponentModel.DataAnnotations;

namespace SupportWay.API.Models
{
    public class RegisterRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(80)]
        public string FullName { get; set; } = string.Empty;
    }
}
