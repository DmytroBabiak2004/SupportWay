using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportWay.API.Services.Implementations;
using SupportWay.Data.Context;
using SupportWay.Data.DTOs;
using SupportWay.Data.Models;
using System.Security.Claims;

namespace SupportWay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private const string StatusPending = "Pending";
        private const string ProviderJar = "MonobankJar";
        private const string ProviderManual = "ManualTransfer";

        private readonly SupportWayContext _db;
        private readonly IConfiguration _config;
        private readonly MonobankJarService _jarService;

        public PaymentsController(
            SupportWayContext db,
            IConfiguration config,
            MonobankJarService jarService)
        {
            _db = db;
            _config = config;
            _jarService = jarService;
        }

        // ── POST /api/payments/donate ──────────────────────────────────────

        [HttpPost("donate")]
        [Authorize]
        public async Task<IActionResult> Donate(
            [FromBody] DonateRequestDto dto, CancellationToken ct)
        {
            if (dto.Amount <= 0)
                return BadRequest("Сума донату має бути більше 0.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("nameid");
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var helpRequest = await _db.HelpRequests
                .FirstOrDefaultAsync(h => h.Id == dto.HelpRequestId, ct);

            if (helpRequest is null) return NotFound("Запит не знайдено.");
            if (!helpRequest.IsActive) return BadRequest("Збір вже завершено.");

            var roundedAmount = decimal.Round(dto.Amount, 2, MidpointRounding.AwayFromZero);

            // Визначаємо jar ID — специфічний для реквесту або дефолтний
            var jarId = !string.IsNullOrWhiteSpace(helpRequest.MonobankJarId)
                ? helpRequest.MonobankJarId
                : _config["Monobank:DefaultJarId"];

            var hasJar = !string.IsNullOrWhiteSpace(jarId);

            // Картка як fallback якщо банки немає
            var card = FirstNonEmpty(
                NormalizeCard(helpRequest.DonationRecipientCardNumber),
                NormalizeCard(_config["App:DefaultDonationRecipientCardNumber"]));

            if (!hasJar && string.IsNullOrWhiteSpace(card))
                return BadRequest(
                    "Для цього збору не налаштовано реквізити. " +
                    "Зверніться до організатора.");

            var statusId = await EnsureStatusAsync(StatusPending, ct);
            var providerId = await EnsureProviderAsync(hasJar ? ProviderJar : ProviderManual, ct);

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                Amount = roundedAmount,
                CreatedAt = DateTime.UtcNow,
                TransactionId = $"sw_{helpRequest.Id:N}_{Guid.NewGuid():N}",
                Comment = dto.Comment?.Trim() ?? $"Донат SupportWay",
                UserId = userId,
                HelpRequestId = helpRequest.Id,
                PaymentStatusId = statusId,
                PaymentProviderId = providerId
            };

            // Якщо немає jar — оновлюємо суму одразу (ручний переказ, довіряємо донору)
            if (!hasJar)
            {
                helpRequest.CollectedAmount += roundedAmount;
                if (helpRequest.TargetAmount > 0 &&
                    helpRequest.CollectedAmount >= helpRequest.TargetAmount)
                    helpRequest.IsActive = false;
            }
            // Якщо є jar — CollectedAmount оновить JarSyncBackgroundService автоматично

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync(ct);

            var recipientName = FirstNonEmpty(
                helpRequest.DonationRecipientName,
                _config["App:DefaultDonationRecipientName"],
                "Отримувач збору");

            var notes = FirstNonEmpty(
                helpRequest.DonationNotes,
                _config["App:DefaultDonationNotes"]);

            if (hasJar)
            {
                var paymentLink = $"https://send.monobank.ua/jar/{jarId}";
                return Ok(new DonateResponseDto
                {
                    PaymentId = payment.Id,
                    Status = StatusPending,
                    PaymentMethod = "payment_link",
                    IsManualTransfer = false,
                    PaymentLink = paymentLink,
                    CardNumber = card,     // fallback якщо банка не відкрилась
                    RecipientName = recipientName,
                    Instructions = $"Натисніть кнопку — відкриється платіжна сторінка Monobank. " +
                                       $"Рекомендована сума: {roundedAmount:0.##} ₴." +
                                       (string.IsNullOrWhiteSpace(notes) ? "" : $" {notes}"),
                    OrderReference = payment.TransactionId
                });
            }

            // Fallback — тільки картка
            var instructions = $"Скопіюйте номер картки та виконайте переказ у банківському застосунку. " +
                               $"Сума: {roundedAmount:0.##} ₴." +
                               (string.IsNullOrWhiteSpace(notes) ? "" : $" {notes}");

            return Ok(new DonateResponseDto
            {
                PaymentId = payment.Id,
                Status = StatusPending,
                PaymentMethod = "bank_card",
                IsManualTransfer = true,
                CardNumber = card,
                RecipientName = recipientName,
                Instructions = instructions,
                OrderReference = payment.TransactionId
            });
        }

        // ── GET /api/payments/{id} ─────────────────────────────────────────

        [HttpGet("{paymentId:guid}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentStatus(
            Guid paymentId, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("nameid");
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var payment = await _db.Payments
                .Include(p => p.PaymentStatus)
                .Include(p => p.PaymentProvider)
                .FirstOrDefaultAsync(p => p.Id == paymentId && p.UserId == userId, ct);

            if (payment is null) return NotFound();

            return Ok(new PaymentStatusDto
            {
                PaymentId = payment.Id,
                Status = payment.PaymentStatus?.NameOfStatus ?? string.Empty,
                Provider = payment.PaymentProvider?.NameOfProvider,
                Amount = payment.Amount,
                HelpRequestId = payment.HelpRequestId,
                CreatedAt = payment.CreatedAt,
                Comment = payment.Comment
            });
        }

        // ── GET /api/payments/my ───────────────────────────────────────────

        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyPayments(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("nameid");
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var payments = await _db.Payments
                .Where(p => p.UserId == userId)
                .Include(p => p.PaymentStatus)
                .Include(p => p.PaymentProvider)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PaymentStatusDto
                {
                    PaymentId = p.Id,
                    Status = p.PaymentStatus != null ? p.PaymentStatus.NameOfStatus : "",
                    Provider = p.PaymentProvider != null ? p.PaymentProvider.NameOfProvider : null,
                    Amount = p.Amount,
                    HelpRequestId = p.HelpRequestId,
                    CreatedAt = p.CreatedAt,
                    Comment = p.Comment
                })
                .ToListAsync(ct);

            return Ok(payments);
        }

        // ── GET /api/payments/jar-info/{jarId} ────────────────────────────
        // Публічний endpoint — фронтенд може показати актуальний баланс банки

        [HttpGet("jar-info/{jarId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetJarInfo(string jarId, CancellationToken ct)
        {
            var info = await _jarService.GetJarInfoAsync(jarId, ct);
            if (info is null)
                return NotFound("Не вдалося отримати інформацію про банку.");

            return Ok(info);
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private async Task<Guid> EnsureStatusAsync(string name, CancellationToken ct)
        {
            var existing = await _db.PaymentStatuses
                .FirstOrDefaultAsync(s => s.NameOfStatus == name, ct);
            if (existing is not null) return existing.Id;

            var status = new PaymentStatus { Id = Guid.NewGuid(), NameOfStatus = name };
            _db.PaymentStatuses.Add(status);
            try { await _db.SaveChangesAsync(ct); }
            catch (DbUpdateException)
            {
                _db.ChangeTracker.Clear();
                return (await _db.PaymentStatuses.FirstAsync(s => s.NameOfStatus == name, ct)).Id;
            }
            return status.Id;
        }

        private async Task<Guid> EnsureProviderAsync(string name, CancellationToken ct)
        {
            var existing = await _db.PaymentProviders
                .FirstOrDefaultAsync(p => p.NameOfProvider == name, ct);
            if (existing is not null) return existing.Id;

            var provider = new PaymentProvider { Id = Guid.NewGuid(), NameOfProvider = name };
            _db.PaymentProviders.Add(provider);
            try { await _db.SaveChangesAsync(ct); }
            catch (DbUpdateException)
            {
                _db.ChangeTracker.Clear();
                return (await _db.PaymentProviders.FirstAsync(p => p.NameOfProvider == name, ct)).Id;
            }
            return provider.Id;
        }

        private static string? Normalize(string? v)
            => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

        private static string? NormalizeCard(string? v)
        {
            if (string.IsNullOrWhiteSpace(v)) return null;
            var digits = new string(v.Where(char.IsDigit).ToArray());
            return string.IsNullOrWhiteSpace(digits) ? null : digits;
        }

        private static string? FirstNonEmpty(params string?[] values)
            => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
    }
}