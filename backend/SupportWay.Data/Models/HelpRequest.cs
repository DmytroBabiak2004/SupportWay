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

        // ── ІСНУЮЧІ (не змінюємо) ──────────────────────────────────────────────
        public ICollection<RequestItem> RequestItems { get; set; } = new List<RequestItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}