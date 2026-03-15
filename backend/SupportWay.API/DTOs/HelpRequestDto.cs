using System.ComponentModel.DataAnnotations;

namespace SupportWay.Data.DTOs
{
    public class HelpRequestDto
    {
        public Guid Id { get; set; }
        public Guid? LocationId { get; set; }
        public string LocationName { get; set; }
        public string StatusName { get; set; }
        public decimal TotalPayments { get; set; }

        public string Title { get; set; } 
        public string Content { get; set; }
        public byte[]? Image { get; set; }
        public DateTime CreatedAt { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; } 

        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
    }

    public class HelpRequestCreateDto
    {
        [Required(ErrorMessage = "Заголовок обов'язковий")]
        [MaxLength(200)]
        public string Title { get; set; }  

        [Required(ErrorMessage = "Опис обов'язковий")]
        public string Content { get; set; }

        public byte[]? Image { get; set; } 
        public Guid? LocationId { get; set; }

    }

    public class RequestStatusDto
    {
        public Guid Id { get; set; }
        public string NameOfStatus { get; set; }
    }

    public class CreateRequestStatusDto
    {
        public string NameOfStatus { get; set; }
    }

    public class SupportTypeDto
    {
        public Guid Id { get; set; }
        public string NameOfType { get; set; }
    }

    public class CreateSupportTypeDto
    {
        public string NameOfType { get; set; }
    }
}