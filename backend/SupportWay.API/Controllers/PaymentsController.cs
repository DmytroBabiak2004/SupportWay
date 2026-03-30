using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportWay.API.Services.Interfaces;
using SupportWay.Data.Context;
using SupportWay.Data.DTOs;
using SupportWay.Data.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SupportWay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly SupportWayContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService,
            SupportWayContext db,
            IConfiguration config,
            ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _db = db;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// POST /api/payments/donate
        /// Ініціює платіж і повертає checkoutUrl для редиректу на сторінку Monobank.
        /// </summary>
        [HttpPost("donate")]
        [Authorize] // Донатити можуть лише авторизовані користувачі
        public async Task<IActionResult> Donate(
            [FromBody] DonateRequestDto dto,
            CancellationToken ct)
        {
            if (dto.Amount <= 0)
                return BadRequest("Сума донату має бути більше 0");

            var helpRequest = await _db.HelpRequests.FindAsync(new object[] { dto.HelpRequestId }, ct);
            if (helpRequest == null)
                return NotFound("Запит не знайдено");

            if (!helpRequest.IsActive)
                return BadRequest("Збір вже завершено");

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var response = await _paymentService.CreateInvoiceAsync(dto, baseUrl, ct);
            return Ok(response);
        }

        /// <summary>
        /// POST /api/payments/webhook/monobank
        /// Monobank надсилає підписаний POST після успішної оплати.
        /// Ендпоінт публічний — перевіряємо підпис самостійно.
        /// </summary>
        [HttpPost("webhook/monobank")]
        [AllowAnonymous]
        public async Task<IActionResult> MonobankWebhook(CancellationToken ct)
        {
            // 1. Читаємо тіло запиту для перевірки підпису і парсингу
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync(ct);
            Request.Body.Position = 0;

            // 2. Перевірка ECDSA-підпису (публічний ключ Monobank)
            //    Захист від підроблених колбеків — обов'язково!
            if (!await VerifySignatureAsync(rawBody, ct))
            {
                _logger.LogWarning("Monobank webhook: невалідний підпис");
                return Unauthorized();
            }

            // 3. Парсимо payload
            using var doc = JsonDocument.Parse(rawBody);
            var root = doc.RootElement;
            var status = root.GetProperty("status").GetString();
            var orderRef = root.GetProperty("reference").GetString();
            var amountKopecks = root.GetProperty("amount").GetInt64();

            // Обробляємо тільки успішні транзакції
            if (status != "success" || string.IsNullOrEmpty(orderRef))
                return Ok(); // Повертаємо 200 — Monobank більше не повторюватиме

            // 4. Витягуємо HelpRequestId з orderRef (формат: sw_<guid32>_<timestamp>)
            if (!TryParseHelpRequestId(orderRef, out var helpRequestId))
            {
                _logger.LogWarning("Не вдалося розпарсити HelpRequestId з reference: {Ref}", orderRef);
                return BadRequest();
            }

            var amount = amountKopecks / 100m;

            // 5. Транзакційне оновлення БД
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                // Ідемпотентність: якщо транзакція вже є — ігноруємо (Monobank може надіслати двічі)
                var alreadyProcessed = await _db.Payments
                    .AnyAsync(p => p.TransactionId == orderRef, ct);

                if (!alreadyProcessed)
                {
                    // Знаходимо або створюємо PaymentStatus "Completed"
                    var statusId = await EnsurePaymentStatusAsync("Completed", ct);
                    var providerId = await EnsurePaymentProviderAsync("Monobank", ct);

                    _db.Payments.Add(new Payment
                    {
                        Id = Guid.NewGuid(),
                        Amount = amount,
                        TransactionId = orderRef,
                        CreatedAt = DateTime.UtcNow,
                        HelpRequestId = helpRequestId,
                        Comment = "Monobank webhook",
                        UserId = "system", // Гостьові платежі без акаунту
                        PaymentStatusId = statusId,
                        PaymentProviderId = providerId
                    });

                    // Оновлюємо CollectedAmount через ExecuteUpdate — один UPDATE без завантаження сутності
                    await _db.HelpRequests
                        .Where(h => h.Id == helpRequestId)
                        .ExecuteUpdateAsync(s =>
                            s.SetProperty(h => h.CollectedAmount,
                                          h => h.CollectedAmount + amount), ct);
                }

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                _logger.LogInformation(
                    "Webhook оброблено: {Ref}, сума {Amount} UAH, HelpRequest {Id}",
                    orderRef, amount, helpRequestId);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                _logger.LogError(ex, "Помилка обробки webhook {Ref}", orderRef);
                return StatusCode(500);
            }

            return Ok();
        }

        // ── Приватні допоміжні методи ────────────────────────────────────────

        /// <summary>
        /// Перевіряє ECDSA-підпис Monobank.
        /// Monobank підписує тіло запиту своїм приватним ключем,
        /// ми перевіряємо публічним ключем з /api/merchant/pubkey.
        /// </summary>
        private async Task<bool> VerifySignatureAsync(string rawBody, CancellationToken ct)
        {
            try
            {
                var signatureHeader = Request.Headers["X-Sign"].FirstOrDefault();
                if (string.IsNullOrEmpty(signatureHeader)) return false;

                // Отримуємо публічний ключ Monobank
                // УВАГА: у продакшні кешуйте цей ключ (IMemoryCache) — він не змінюється часто
                using var http = new HttpClient();
                http.DefaultRequestHeaders.Add("X-Token", _config["Monobank:Token"]);
                var pubKeyJson = await http.GetStringAsync(
                    "https://api.monobank.ua/api/merchant/pubkey", ct);

                using var doc = JsonDocument.Parse(pubKeyJson);
                var pubKeyBase64 = doc.RootElement.GetProperty("key").GetString()!;
                var pubKeyBytes = Convert.FromBase64String(pubKeyBase64);
                var signatureBytes = Convert.FromBase64String(signatureHeader);
                var bodyBytes = Encoding.UTF8.GetBytes(rawBody);

                using var ecdsa = ECDsa.Create();
                ecdsa.ImportSubjectPublicKeyInfo(pubKeyBytes, out _);

                // Monobank використовує SHA256withECDSA
                return ecdsa.VerifyData(bodyBytes, signatureBytes, HashAlgorithmName.SHA256);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка верифікації підпису Monobank");
                return false;
            }
        }

        /// <summary>Парсить HelpRequestId із рядка формату sw_guid32_timestamp.</summary>
        private static bool TryParseHelpRequestId(string reference, out Guid id)
        {
            // sw_ → частини: ["sw", "<guid>", "<ts>"]
            var parts = reference.Split('_');
            if (parts.Length >= 3 && Guid.TryParseExact(parts[1], "N", out id))
                return true;
            id = Guid.Empty;
            return false;
        }

        private async Task<Guid> EnsurePaymentStatusAsync(string name, CancellationToken ct)
        {
            var existing = await _db.PaymentStatuses
                .FirstOrDefaultAsync(s => s.NameOfStatus == name, ct);
            if (existing != null) return existing.Id;

            var newStatus = new PaymentStatus { Id = Guid.NewGuid(), NameOfStatus = name };
            _db.PaymentStatuses.Add(newStatus);
            await _db.SaveChangesAsync(ct);
            return newStatus.Id;
        }

        private async Task<Guid> EnsurePaymentProviderAsync(string name, CancellationToken ct)
        {
            var existing = await _db.PaymentProviders
                .FirstOrDefaultAsync(p => p.NameOfProvider == name, ct);
            if (existing != null) return existing.Id;

            var newProvider = new PaymentProvider { Id = Guid.NewGuid(), NameOfProvider = name };
            _db.PaymentProviders.Add(newProvider);
            await _db.SaveChangesAsync(ct);
            return newProvider.Id;
        }
    }
}