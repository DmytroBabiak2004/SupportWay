using MongoDB.Driver;
using SupportWay.Data.Mongo;
using SupportWay.Data.Mongo.Documents;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.Data.Repositories.Implementations;

public class MongoMessagesRepository : IMongoMessagesRepository
{
    private readonly MongoChatContext _context;

    public MongoMessagesRepository(MongoChatContext context)
    {
        _context = context;
    }

    public async Task<MessageDocument> CreateMessageAsync(MessageDocument message)
    {
        await _context.Messages.InsertOneAsync(message);
        return message;
    }

    public async Task<List<MessageDocument>> GetChatMessagesAsync(Guid chatId, int limit = 50, int skip = 0)
    {
        return await _context.Messages
            .Find(m => m.ChatId == chatId && !m.IsDeleted)
            .SortBy(m => m.SentAt)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<MessageDocument?> GetByIdAsync(Guid messageId)
    {
        return await _context.Messages
            .Find(m => m.Id == messageId)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(MessageDocument message)
    {
        await _context.Messages.ReplaceOneAsync(
            m => m.Id == message.Id,
            message);
    }

    public async Task MarkAsReadAsync(Guid messageId, string userId)
    {
        var update = Builders<MessageDocument>.Update
            .Set(m => m.IsRead, true)
            .AddToSet(m => m.ReadByUserIds, userId);

        await _context.Messages.UpdateOneAsync(
            m => m.Id == messageId,
            update);
    }

    public async Task MarkChatAsReadUpToAsync(Guid chatId, string userId, DateTime sentAt)
    {
        var filter = Builders<MessageDocument>.Filter.And(
            Builders<MessageDocument>.Filter.Eq(m => m.ChatId, chatId),
            Builders<MessageDocument>.Filter.Lte(m => m.SentAt, sentAt),
            Builders<MessageDocument>.Filter.Ne(m => m.SenderId, userId),
            Builders<MessageDocument>.Filter.Eq(m => m.IsDeleted, false)
        );

        var update = Builders<MessageDocument>.Update
            .Set(m => m.IsRead, true)
            .AddToSet(m => m.ReadByUserIds, userId);

        await _context.Messages.UpdateManyAsync(filter, update);
    }

    public async Task DeleteMessageAsync(Guid messageId)
    {
        var update = Builders<MessageDocument>.Update
            .Set(m => m.IsDeleted, true)
            .Set(m => m.EditedAt, DateTime.UtcNow);

        await _context.Messages.UpdateOneAsync(
            m => m.Id == messageId,
            update);
    }
}