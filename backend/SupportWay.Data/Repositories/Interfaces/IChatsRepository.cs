using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IChatsRepository
    {
        Task<Chat?> GetByIdAsync(Guid chatId);
        Task<IEnumerable<Chat>> GetChatsByUserIdAsync(string userId);
        Task AddAsync(Chat chat);
        Task DeleteAsync(Chat chat);
        Task<bool> IsUserInChatAsync(Guid chatId, string userId);

        Task<Chat?> GetPrivateChatByParticipantsAsync(string user1Id, string user2Id);
        Task<IReadOnlyList<string>> GetParticipantUserIdsAsync(Guid chatId);
    }
}
