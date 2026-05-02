namespace SupportWay.Data.DTOs
{
    public class MapMarkerDto
    {
        public Guid RequestItemId { get; set; }
        public Guid HelpRequestId { get; set; }
        public string RequestItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public Guid SupportTypeId { get; set; }
        public string SupportTypeName { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string LocationAddress { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ShortContent { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CollectedAmount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
    }

    public class MapFilterParams
    {
        public Guid? SupportTypeId { get; set; }
        public bool? IsActive { get; set; }
        public string? Region { get; set; }
        public decimal? MinCollectedAmount { get; set; }
        public decimal? MaxTargetAmount { get; set; }
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 200;
    }

    public class DonateRequestDto
    {
        public Guid HelpRequestId { get; set; }
        public decimal Amount { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class DonateResponseDto
    {
        public Guid PaymentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string? CheckoutUrl { get; set; }
        public string? OrderReference { get; set; }
        public string? RecipientName { get; set; }
        public string? CardNumber { get; set; }
        public string? Iban { get; set; }
        public string? PaymentLink { get; set; }
        public string? Instructions { get; set; }
        public bool IsManualTransfer { get; set; }
    }

    public class PaymentStatusDto
    {
        public Guid PaymentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public Guid? HelpRequestId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Provider { get; set; }
        public string? Comment { get; set; }
        public string? CheckoutUrl { get; set; }
    }

    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int Total { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
    }
}
