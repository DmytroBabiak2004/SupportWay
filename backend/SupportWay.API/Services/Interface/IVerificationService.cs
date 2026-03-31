using SupportWay.API.DTOs;
using SupportWay.Data.Models;

namespace SupportWay.API.Services.Interface
{
    public interface IVerificationService
    {
        Task<VerificationRequestDto?> GetMyPendingRequestAsync(string userId);
        Task<Guid> SubmitRequestAsync(string userId, SubmitVerificationDto dto);
        Task<IEnumerable<VerificationRequestDto>> GetAllAsync(VerificationStatus? status = null);
        Task DecideAsync(Guid id, DecideVerificationDto dto, string adminId);
    }
}
