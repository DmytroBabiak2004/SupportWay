using SupportWay.Data.Mongo.Documents;

namespace SupportWay.Data.Repositories.Interfaces;

public interface IMongoChatsRepository
{
    Task<ChatDocument> CreateChatAsync(ChatDocument chat);

    Task<ChatDocument?> GetByIdAsync(Guid chatId);

    Task<List<ChatDocument>> GetUserChatsAsync(string userId);

    Task AddParticipantAsync(Guid chatId, ChatParticipantDocument participant);

    Task RemoveParticipantAsync(Guid chatId, string userId);

    Task UpdateLastMessageAsync(Guid chatId, Guid messageId);

    Task<bool> IsUserInChatAsync(Guid chatId, string userId);
}