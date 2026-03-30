using System.Text;
using System.Text.Json;
using SupportWay.Data.DTOs;
using SupportWay.API.Services.Interfaces;

namespace SupportWay.API.Services.Implementations
{
    public class MonobankPaymentService : IPaymentService
    {
        private readonly HttpClient _http;
        private readonly string _token;
        private readonly ILogger<MonobankPaymentService> _logger;

        public MonobankPaymentService(
            HttpClient http,
            IConfiguration config,
            ILogger<MonobankPaymentService> logger)
        {
            _http = http;
            _logger = logger;
            _token = config["Monobank:Token"]
                      ?? throw new InvalidOperationException(
                             "Monobank:Token не налаштований у конфігурації");
        }

        public async Task<DonateResponseDto> CreateInvoiceAsync(
            DonateRequestDto request,
            string callbackBaseUrl,
            CancellationToken ct = default)
        {
            // Monobank приймає суму в копійках (int)
            var amountKopecks = (long)(request.Amount * 100);

            // Унікальний ідентифікатор замовлення — використовуємо для парсингу у webhook
            // Формат: sw_<HelpRequestId без дефісів>_<unix timestamp>
            var orderRef = $"sw_{request.HelpRequestId:N}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

            var body = new
            {
                amount = amountKopecks,
                ccy = 980, // UAH
                merchantPaymInfo = new
                {
                    reference = orderRef,
                    destination = string.IsNullOrWhiteSpace(request.Comment)
                                  ? "Донат на збір SupportWay"
                                  : request.Comment
                },
                redirectUrl = $"{callbackBaseUrl}/donate/success",
                webHookUrl = $"{callbackBaseUrl}/api/payments/webhook/monobank"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            _http.DefaultRequestHeaders.Remove("X-Token");
            _http.DefaultRequestHeaders.Add("X-Token", _token);

            var response = await _http.PostAsync(
                "https://api.monobank.ua/api/merchant/invoice/create", content, ct);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Monobank invoice error: {Error}", err);
                throw new InvalidOperationException($"Monobank API error: {err}");
            }

            var resultJson = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(resultJson);

            return new DonateResponseDto
            {
                CheckoutUrl = doc.RootElement.GetProperty("pageUrl").GetString()!,
                OrderReference = orderRef
            };
        }
    }
}