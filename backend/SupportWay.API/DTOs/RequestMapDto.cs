using SupportWay.Data.Models;

namespace SupportWay.API.DTOs
{
    public class RequestMapDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Region { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CollectedAmount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<SupportType> SupportTypes { get; set; } = new();
    }

    public class MapFilterParams
    {

        public Guid? SupportTypeId { get; set; }

        public bool? IsActive { get; set; }

        public string? Region { get; set; }

        public decimal? MaxTarget { get; set; }

        public decimal? MinCollected { get; set; }

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
        public string CheckoutUrl { get; set; } = string.Empty;
        public string OrderReference { get; set; } = string.Empty;
    }

    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int Total { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
    }
}