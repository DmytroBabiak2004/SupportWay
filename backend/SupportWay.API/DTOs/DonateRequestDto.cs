namespace SupportWay.Data.DTOs
{
    public class DonateRequestDto
    {
        public Guid HelpRequestId { get; set; }
        public decimal Amount { get; set; }
        public string? Comment { get; set; }
    }
}