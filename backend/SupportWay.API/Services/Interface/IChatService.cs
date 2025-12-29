using SupportWay.Data.Models;

public interface IChatService
{
    Task<IEnumerable<Chat>> GetChatsByUserIdAsync(string userId);
    Task<Chat?> GetByIdAsync(Guid id);
    Task<ChatDto> AddChatAsync(string user1Id, string user2Id);
    Task DeleteChatAsync(Guid chatId);
    Task<bool> IsUserInChatAsync(Guid chatId, string userId);
}
