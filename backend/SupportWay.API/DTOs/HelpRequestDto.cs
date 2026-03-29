using System.ComponentModel.DataAnnotations;

namespace SupportWay.Data.DTOs
{
    public class HelpRequestDto
    {
        public Guid Id { get; set; }
        public Guid? LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public decimal TotalPayments { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public byte[]? Image { get; set; }
        public DateTime CreatedAt { get; set; }

        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }

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
    }

    public class RequestStatusDto
    {
        public Guid Id { get; set; }
        public string NameOfStatus { get; set; } = string.Empty;
    }

    public class CreateRequestStatusDto
    {
        public string NameOfStatus { get; set; } = string.Empty;
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