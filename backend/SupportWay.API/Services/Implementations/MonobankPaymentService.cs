using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SupportWay.API.Services.Interfaces;
using SupportWay.Data.DTOs;
using SupportWay.Data.Models;

namespace SupportWay.API.Services.Implementations
{
    public class MonobankPaymentService : IPaymentService
    {
        private readonly HttpClient _http;
        private readonly ILogger<MonobankPaymentService> _logger;
        private readonly string? _token;
        private readonly string _apiBaseUrl;
        private readonly string? _publicBaseUrl;

        public MonobankPaymentService(
            HttpClient http,
            IConfiguration config,
            ILogger<MonobankPaymentService> logger)
        {
            _http = http;
            _logger = logger;
            _token = config["Monobank:Token"];
            _apiBaseUrl = (config["Monobank:ApiBaseUrl"] ?? "https://api.monobank.ua").TrimEnd('/');
            _publicBaseUrl = config["Monobank:PublicBaseUrl"]?.TrimEnd('/');
        }

        public async Task<DonateResponseDto> CreateInvoiceAsync(
            DonateRequestDto request,
            HelpRequest helpRequest,
            Payment payment,
            string callbackBaseUrl,
            CancellationToken ct = default)
        {
            EnsureConfiguration();

            var amountKopecks = decimal.ToInt64(decimal.Round(request.Amount * 100m, 0, MidpointRounding.AwayFromZero));
            var effectiveBaseUrl = !string.IsNullOrWhiteSpace(_publicBaseUrl)
                ? _publicBaseUrl!
                : callbackBaseUrl.TrimEnd('/');

            var description = string.IsNullOrWhiteSpace(request.Comment)
                ? $"Донат на збір {helpRequest.Id:N}"
                : request.Comment.Trim();

            var body = new
            {
                amount = amountKopecks,
                ccy = 980,
                merchantPaymInfo = new
                {
                    reference = payment.TransactionId,
                    destination = description,
                    comment = description
                },
                redirectUrl = $"{effectiveBaseUrl}/payments/result?paymentId={payment.Id}",
                webHookUrl = $"{effectiveBaseUrl}/api/payments/webhook/monobank",
                validity = 86400,
                paymentType = "debit"
            };

            using var message = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/api/merchant/invoice/create");
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            message.Headers.Add("X-Token", _token);
            message.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(message, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Monobank invoice error {StatusCode}: {Body}", (int)response.StatusCode, responseBody);
                throw new InvalidOperationException($"Monobank API error: {responseBody}");
            }

            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;
            var invoiceId = root.TryGetProperty("invoiceId", out var invoiceProp) ? invoiceProp.GetString() : null;
            var pageUrl = root.TryGetProperty("pageUrl", out var pageProp) ? pageProp.GetString() : null;

            payment.Comment = BuildComment(payment.Comment, invoiceId, pageUrl);

            return new DonateResponseDto
            {
                PaymentId = payment.Id,
                PaymentMethod = "monobank_checkout",
                Status = "Pending",
                CheckoutUrl = pageUrl,
                OrderReference = payment.TransactionId,
                Instructions = "Користувача потрібно переадресувати на hosted checkout Monobank. Дані картки донора не повинні вводитись у вашому застосунку.",
                IsManualTransfer = false
            };
        }

        private void EnsureConfiguration()
        {
            if (string.IsNullOrWhiteSpace(_token) || _token == "YOUR_MONOBANK_MERCHANT_TOKEN")
            {
                throw new InvalidOperationException("Monobank acquiring token is not configured. Add a valid Monobank:Token from web.monobank.ua.");
            }
        }

        private static string BuildComment(string? currentComment, string? invoiceId, string? pageUrl)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(currentComment)) parts.Add(currentComment.Trim());
            if (!string.IsNullOrWhiteSpace(invoiceId)) parts.Add($"invoiceId={invoiceId}");
            if (!string.IsNullOrWhiteSpace(pageUrl)) parts.Add($"checkoutUrl={pageUrl}");
            return string.Join(" | ", parts);
        }
    }
}
