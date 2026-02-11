using SupportWay.API.DTOs;

namespace SupportWay.API.Services.Interface
{
    public interface IProfileService
    {
        Task<ProfileDto?> GetProfileAsync(string userId);
        Task<ProfileDto?> GetProfileByUsernameAsync(string username);
        Task AddProfileAsync(string userId);
        Task AddProfileAsync(string userId, string? name, string? fullName);
        Task UpdateDescriptionAsync(string userId, string description);
        Task UpdatePhotoAsync(string userId, byte[] photo);
        Task UpdateNameAsync(string userId, string? name, string? fullName);
    }

}
