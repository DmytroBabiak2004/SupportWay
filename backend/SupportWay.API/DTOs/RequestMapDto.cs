namespace SupportWay.API.DTOs
{

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
}