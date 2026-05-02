using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SupportWay.Data.DTOs
{
    public class CreateHelpRequestRequest
    {
        [Required(ErrorMessage = "Заголовок обов'язковий")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Опис обов'язковий")]
        public string Content { get; set; } = string.Empty;

        public IFormFile? Image { get; set; }

        public Guid? LocationId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Address { get; set; }
        public string? DistrictName { get; set; }

        [MaxLength(64)]
        public string? PreferredDonationMethod { get; set; }

        [MaxLength(200)]
        public string? DonationRecipientName { get; set; }

        [MaxLength(32)]
        public string? DonationRecipientCardNumber { get; set; }

        [MaxLength(64)]
        public string? DonationRecipientIban { get; set; }

        [MaxLength(500)]
        public string? DonationPaymentLink { get; set; }

        [MaxLength(1000)]
        public string? DonationNotes { get; set; }
    }
}
