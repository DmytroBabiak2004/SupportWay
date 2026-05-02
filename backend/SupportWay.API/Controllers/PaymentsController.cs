using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private const string AwaitingManualTransferStatus = "AwaitingManualTransfer";
        private const string ManualProvider = "ManualTransfer";

        private readonly SupportWayContext _db;
        private readonly IConfiguration _config;

        public PaymentsController(SupportWayContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [HttpPost("donate")]
        [Authorize]
        public async Task<IActionResult> Donate([FromBody] DonateRequestDto dto, CancellationToken ct)
        {
            if (dto.Amount <= 0)
                return BadRequest("Сума донату має бути більше 0.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var helpRequest = await _db.HelpRequests
                .FirstOrDefaultAsync(h => h.Id == dto.HelpRequestId, ct);

            if (helpRequest == null)
                return NotFound("Запит не знайдено.");

            if (!helpRequest.IsActive)
                return BadRequest("Збір вже завершено.");

            var destination = ResolveDonationDestination(helpRequest, dto.Amount);
            if (destination == null)
            {
                return BadRequest("Для цього збору не налаштовано реквізити для донату. Додайте картку, IBAN або платіжне посилання в реквесті або в appsettings як дефолтні реквізити.");
            }

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                Amount = decimal.Round(dto.Amount, 2, MidpointRounding.AwayFromZero),
                CreatedAt = DateTime.UtcNow,
                TransactionId = BuildTransactionReference(dto.HelpRequestId),
                Comment = BuildPaymentComment(dto.Comment, destination),
                UserId = userId,
                HelpRequestId = helpRequest.Id,
                PaymentStatusId = await EnsurePaymentStatusAsync(AwaitingManualTransferStatus, ct),
                PaymentProviderId = await EnsurePaymentProviderAsync(ManualProvider, ct)
            };

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync(ct);

            return Ok(new DonateResponseDto
            {
                PaymentId = payment.Id,
                Status = AwaitingManualTransferStatus,
                PaymentMethod = destination.PaymentMethod,
                RecipientName = destination.RecipientName,
                CardNumber = destination.CardNumber,
                Iban = destination.Iban,
                PaymentLink = destination.PaymentLink,
                Instructions = destination.Instructions,
                IsManualTransfer = true,
                OrderReference = payment.TransactionId
            });
        }

        [HttpGet("{paymentId:guid}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentStatus(Guid paymentId, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var payment = await _db.Payments
                .Include(p => p.PaymentStatus)
                .Include(p => p.PaymentProvider)
                .FirstOrDefaultAsync(p => p.Id == paymentId && p.UserId == userId, ct);

            if (payment == null)
                return NotFound();

            return Ok(new PaymentStatusDto
            {
                PaymentId = payment.Id,
                Status = payment.PaymentStatus?.NameOfStatus ?? string.Empty,
                Provider = payment.PaymentProvider?.NameOfProvider,
                Amount = payment.Amount,
                HelpRequestId = payment.HelpRequestId,
                CreatedAt = payment.CreatedAt,
                Comment = payment.Comment,
                CheckoutUrl = null
            });
        }

        private DonationDestination? ResolveDonationDestination(HelpRequest helpRequest, decimal donationAmount)
        {
            var recipientName = FirstNonEmpty(
                Normalize(helpRequest.DonationRecipientName),
                Normalize(_config["App:DefaultDonationRecipientName"]),
                "Отримувач збору");

            var card = FirstNonEmpty(
                NormalizeCard(helpRequest.DonationRecipientCardNumber),
                NormalizeCard(_config["App:DefaultDonationRecipientCardNumber"]));

            var iban = FirstNonEmpty(
                Normalize(helpRequest.DonationRecipientIban)?.ToUpperInvariant(),
                Normalize(_config["App:DefaultDonationRecipientIban"])?.ToUpperInvariant());

            var link = FirstNonEmpty(
                Normalize(helpRequest.DonationPaymentLink),
                Normalize(_config["App:DefaultDonationLink"]));

            var preferredMethod = Normalize(helpRequest.PreferredDonationMethod)?.ToLowerInvariant();
            var effectiveMethod = preferredMethod switch
            {
                "bank_card" when !string.IsNullOrWhiteSpace(card) => "bank_card",
                "iban" when !string.IsNullOrWhiteSpace(iban) => "iban",
                "payment_link" when !string.IsNullOrWhiteSpace(link) => "payment_link",
                _ when !string.IsNullOrWhiteSpace(card) => "bank_card",
                _ when !string.IsNullOrWhiteSpace(link) => "payment_link",
                _ when !string.IsNullOrWhiteSpace(iban) => "iban",
                _ => null
            };

            if (effectiveMethod == null)
                return null;

            var coreInstruction = effectiveMethod switch
            {
                "bank_card" => "Скопіюйте номер картки, виконайте переказ у своєму банківському застосунку та за потреби вкажіть коментар до платежу.",
                "iban" => "Скопіюйте IBAN, виконайте переказ у своєму банківському застосунку та за потреби вкажіть коментар до платежу.",
                _ => "Відкрийте платіжне посилання та завершіть переказ у своєму банківському застосунку."
            };

            var notes = Normalize(helpRequest.DonationNotes);
            var instructions = $"{coreInstruction} Рекомендована сума цього донату: {decimal.Round(donationAmount, 2, MidpointRounding.AwayFromZero):0.##} ₴. Реквізити прив’язані до цього запиту.";
            if (!string.IsNullOrWhiteSpace(notes))
                instructions += $" Додатково: {notes}.";

            return new DonationDestination
            {
                PaymentMethod = effectiveMethod,
                RecipientName = recipientName!,
                CardNumber = card,
                Iban = iban,
                PaymentLink = link,
                Instructions = instructions
            };
        }

        private async Task<Guid> EnsurePaymentStatusAsync(string name, CancellationToken ct)
        {
            var existing = await _db.PaymentStatuses.FirstOrDefaultAsync(s => s.NameOfStatus == name, ct);
            if (existing != null) return existing.Id;

            var status = new PaymentStatus { Id = Guid.NewGuid(), NameOfStatus = name };
            _db.PaymentStatuses.Add(status);
            await _db.SaveChangesAsync(ct);
            return status.Id;
        }

        private async Task<Guid> EnsurePaymentProviderAsync(string name, CancellationToken ct)
        {
            var existing = await _db.PaymentProviders.FirstOrDefaultAsync(p => p.NameOfProvider == name, ct);
            if (existing != null) return existing.Id;

            var provider = new PaymentProvider { Id = Guid.NewGuid(), NameOfProvider = name };
            _db.PaymentProviders.Add(provider);
            await _db.SaveChangesAsync(ct);
            return provider.Id;
        }

        private static string BuildTransactionReference(Guid helpRequestId)
            => $"sw_manual_{helpRequestId:N}_{Guid.NewGuid():N}";

        private static string BuildPaymentComment(string? userComment, DonationDestination destination)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(userComment)) parts.Add(userComment.Trim());
            parts.Add($"method={destination.PaymentMethod}");
            if (!string.IsNullOrWhiteSpace(destination.CardNumber)) parts.Add($"card={destination.CardNumber}");
            if (!string.IsNullOrWhiteSpace(destination.Iban)) parts.Add($"iban={destination.Iban}");
            if (!string.IsNullOrWhiteSpace(destination.PaymentLink)) parts.Add($"paymentLink={destination.PaymentLink}");
            return string.Join(" | ", parts);
        }

        private static string? Normalize(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string? NormalizeCard(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var digits = new string(value.Where(char.IsDigit).ToArray());
            return string.IsNullOrWhiteSpace(digits) ? null : digits;
        }

        private static string? FirstNonEmpty(params string?[] values)
            => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

        private sealed class DonationDestination
        {
            public string PaymentMethod { get; init; } = string.Empty;
            public string RecipientName { get; init; } = string.Empty;
            public string? CardNumber { get; init; }
            public string? Iban { get; init; }
            public string? PaymentLink { get; init; }
            public string Instructions { get; init; } = string.Empty;
        }
    }
}
