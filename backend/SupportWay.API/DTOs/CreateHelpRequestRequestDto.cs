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
        public Guid? LocationId { get; set; }
    }
}