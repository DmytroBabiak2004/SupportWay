using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IChatsRepository
    {
        Task<Chat?> GetByIdAsync(int chatId);
        Task<IEnumerable<Chat>> GetChatsByUserIdAsync(string userId);
        Task AddAsync(Chat chat);
        Task DeleteAsync(Chat chat);
        Task<bool> IsUserInChatAsync(int chatId, string userId);
    }

}
