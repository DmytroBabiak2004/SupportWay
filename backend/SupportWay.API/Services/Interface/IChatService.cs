using SupportWay.API.DTOs;
using SupportWay.Data.Models;

public interface IChatService
{
    Task<IEnumerable<ChatListItemDto>> GetChatsByUserIdAsync(string userId);
    Task<Chat?> GetByIdAsync(Guid id);
    Task<ChatListItemDto> GetOrCreatePrivateChatAsync(string user1Id, string user2Id);
    Task DeleteChatAsync(Guid chatId);
    Task<bool> IsUserInChatAsync(Guid chatId, string userId);
    Task<IReadOnlyList<string>> GetParticipantUserIdsAsync(Guid chatId);
}
