using SupportWay.Data.Models;

namespace SupportWay.Services.Interfaces
{
    public interface IChatService
    {
        Task<Chat?> GetByIdAsync(int chatId);
        Task<IEnumerable<Chat>> GetChatsByUserIdAsync(string userId);
        Task AddChatAsync(Chat chat);
        Task DeleteChatAsync(int chatId);
        Task<bool> IsUserInChatAsync(int chatId, string userId);
    }
}
