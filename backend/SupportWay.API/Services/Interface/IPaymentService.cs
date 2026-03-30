using SupportWay.Data.DTOs;

namespace SupportWay.API.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<DonateResponseDto> CreateInvoiceAsync(
            DonateRequestDto request,
            string callbackBaseUrl,
            CancellationToken ct = default);
    }
}