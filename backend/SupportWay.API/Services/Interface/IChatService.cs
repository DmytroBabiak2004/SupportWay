using SupportWay.Data.Models;

namespace SupportWay.Services.Interfaces
{
    public interface IChatService
    {
        Task<Chat?> GetByIdAsync(Guid chatId);
        Task<IEnumerable<Chat>> GetChatsByUserIdAsync(string userId);
        Task AddChatAsync(Chat chat);
        Task DeleteChatAsync(Guid chatId);
        Task<bool> IsUserInChatAsync(Guid chatId, string userId);
    }
}
