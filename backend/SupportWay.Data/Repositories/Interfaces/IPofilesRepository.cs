using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IProfilesRepository
    {
        Task<Profile?> GetByUserIdAsync(string userId);
        Task AddAsync(Profile profile);
        Task<bool> ExistsAsync(string userId);

        Task UpdateDescriptionAsync(string userId, string description);
        Task UpdatePhotoAsync(string userId, byte[] photo);
    }
}
