using SupportWay.Data.DTOs;
using SupportWay.Data.Models;

namespace SupportWay.API.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<DonateResponseDto> CreateInvoiceAsync(
            DonateRequestDto request,
            HelpRequest helpRequest,
            Payment payment,
            string callbackBaseUrl,
            CancellationToken ct = default);
    }
}
