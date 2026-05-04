using SupportWay.Data.Mongo.Documents;

namespace SupportWay.Data.Repositories.Interfaces;

public interface IMongoMessagesRepository
{
    Task<MessageDocument> CreateMessageAsync(MessageDocument message);

    Task<List<MessageDocument>> GetChatMessagesAsync(Guid chatId, int limit = 50, int skip = 0);

    Task<MessageDocument?> GetByIdAsync(Guid messageId);

    Task UpdateAsync(MessageDocument message);

    Task MarkAsReadAsync(Guid messageId, string userId);

    Task MarkChatAsReadUpToAsync(Guid chatId, string userId, DateTime sentAt);

    Task DeleteMessageAsync(Guid messageId);
}