namespace SupportWay.Data.Models
{
    public class HelpRequest : Post
    {
        public Guid? LocationId { get; set; }
        public Location? Location { get; set; }

        public decimal TargetAmount { get; set; }
        public decimal CollectedAmount { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public bool IsActive { get; set; } = true;

        // Donation destination details selected by the request author.
        // NOTE: storing raw card numbers is not ideal for production PCI/security posture;
        // it is used here only as an explicit user-provided payout destination.
        public string? PreferredDonationMethod { get; set; }
        public string? DonationRecipientName { get; set; }
        public string? DonationRecipientCardNumber { get; set; }
        public string? DonationRecipientIban { get; set; }
        public string? DonationPaymentLink { get; set; }
        public string? DonationNotes { get; set; }

        public ICollection<RequestItem> RequestItems { get; set; } = new List<RequestItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
