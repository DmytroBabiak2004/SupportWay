namespace SupportWay.Data.DTOs
{
    public class DonateResponseDto
    {
        public Guid PaymentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// Для Monobank Checkout — URL для редіректу. Для ручного переказу — null.
        /// </summary>
        public string? CheckoutUrl { get; set; }

        public string? OrderReference { get; set; }

        // Реквізити для ручного переказу
        public string? RecipientName { get; set; }
        public string? CardNumber { get; set; }
        public string? Iban { get; set; }
        public string? PaymentLink { get; set; }
        public string? Instructions { get; set; }

        public bool IsManualTransfer { get; set; }
    }
}