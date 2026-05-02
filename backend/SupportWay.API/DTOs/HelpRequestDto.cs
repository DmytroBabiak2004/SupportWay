using System.ComponentModel.DataAnnotations;

namespace SupportWay.API.DTOs
{
    public class HelpRequestDetailsDto
    {
        public Guid Id { get; set; }
        public Guid? LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string LocationAddress { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? ImageBase64 { get; set; }
        public DateTime CreatedAt { get; set; }

        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? AuthorPhotoBase64 { get; set; }

        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }

        public decimal TargetAmount { get; set; }
        public decimal CollectedAmount { get; set; }
        public decimal TotalPayments { get; set; }
        public bool IsActive { get; set; }
        public int ProgressPercent { get; set; }

        public string? PreferredDonationMethod { get; set; }
        public string? DonationRecipientName { get; set; }
        public string? DonationRecipientCardNumber { get; set; }
        public string? DonationRecipientIban { get; set; }
        public string? DonationPaymentLink { get; set; }
        public string? DonationNotes { get; set; }

        public List<RequestItemDetailsDto> RequestItems { get; set; } = new();
    }

    public class RequestItemDetailsDto
    {
        public Guid Id { get; set; }
        public Guid HelpRequestId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public Guid SupportTypeId { get; set; }
        public string SupportTypeName { get; set; } = string.Empty;
    }

    public class HelpRequestDto
    {
        public Guid Id { get; set; }
        public Guid? LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string? LocationAddress { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public decimal TotalPayments { get; set; }
        public string Content { get; set; } = string.Empty;
        public byte[]? Image { get; set; }
        public DateTime CreatedAt { get; set; }

        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }

        public decimal TargetAmount { get; set; }
        public decimal CollectedAmount { get; set; }
        public bool IsActive { get; set; }

        public string? PreferredDonationMethod { get; set; }
        public string? DonationRecipientName { get; set; }
        public string? DonationRecipientCardNumber { get; set; }
        public string? DonationRecipientIban { get; set; }
        public string? DonationPaymentLink { get; set; }
        public string? DonationNotes { get; set; }

        public List<RequestItemDto> RequestItems { get; set; } = new();
    }

    public class RequestItemDto
    {
        public Guid Id { get; set; }
        public Guid HelpRequestId { get; set; }
        public Guid SupportTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string SupportTypeName { get; set; } = string.Empty;
    }

    public class HelpRequestCreateDto
    {
        [Required(ErrorMessage = "Заголовок обов'язковий")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Опис обов'язковий")]
        public string Content { get; set; } = string.Empty;

        public byte[]? Image { get; set; }
        public Guid? LocationId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Address { get; set; }
        public string? DistrictName { get; set; }

        public string? PreferredDonationMethod { get; set; }
        public string? DonationRecipientName { get; set; }
        public string? DonationRecipientCardNumber { get; set; }
        public string? DonationRecipientIban { get; set; }
        public string? DonationPaymentLink { get; set; }
        public string? DonationNotes { get; set; }
    }

    public class SupportTypeDto
    {
        public Guid Id { get; set; }
        public string NameOfType { get; set; } = string.Empty;
    }

    public class CreateSupportTypeDto
    {
        public string NameOfType { get; set; } = string.Empty;
    }
}
