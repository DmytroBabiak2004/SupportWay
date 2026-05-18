namespace SupportWay.Data.DTOs
{
    public class PaymentStatusDto
    {
        public Guid PaymentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Provider { get; set; }
        public decimal Amount { get; set; }
        public Guid? HelpRequestId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Comment { get; set; }
        public string? CheckoutUrl { get; set; }
    }
}