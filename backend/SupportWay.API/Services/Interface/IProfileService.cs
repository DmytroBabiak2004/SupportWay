using SupportWay.API.DTOs;

namespace SupportWay.API.Services.Interface
{
    public interface IProfileService
    {
        Task<ProfileDto?> GetProfileAsync(string userId);
        Task AddProfileAsync(string userId);
        Task UpdateDescriptionAsync(string userId, string description);
        Task UpdatePhotoAsync(string userId, byte[] photo);
    }

}
