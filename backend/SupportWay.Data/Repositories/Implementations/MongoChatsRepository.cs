using MongoDB.Driver;
using SupportWay.Data.Mongo;
using SupportWay.Data.Mongo.Documents;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.Data.Repositories.Implementations;

public class MongoChatsRepository : IMongoChatsRepository
{
    private readonly MongoChatContext _context;

    public MongoChatsRepository(MongoChatContext context)
    {
        _context = context;
    }

    public async Task<ChatDocument> CreateChatAsync(ChatDocument chat)
    {
        await _context.Chats.InsertOneAsync(chat);
        return chat;
    }

    public async Task<ChatDocument?> GetByIdAsync(Guid chatId)
    {
        return await _context.Chats
            .Find(c => c.Id == chatId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<ChatDocument>> GetUserChatsAsync(string userId)
    {
        return await _context.Chats
            .Find(c => c.Participants.Any(p => p.UserId == userId))
            .SortByDescending(c => c.LastMessageAt)
            .ThenByDescending(c => c.StartedAt)
            .ToListAsync();
    }

    public async Task AddParticipantAsync(Guid chatId, ChatParticipantDocument participant)
    {
        participant.ChatId = chatId;

        var update = Builders<ChatDocument>.Update
            .AddToSet(c => c.Participants, participant);

        await _context.Chats.UpdateOneAsync(
            c => c.Id == chatId,
            update);
    }

    public async Task RemoveParticipantAsync(Guid chatId, string userId)
    {
        var update = Builders<ChatDocument>.Update
            .PullFilter(c => c.Participants, p => p.UserId == userId);

        await _context.Chats.UpdateOneAsync(
            c => c.Id == chatId,
            update);
    }

    public async Task UpdateLastMessageAsync(Guid chatId, Guid messageId)
    {
        var message = await _context.Messages
            .Find(m => m.Id == messageId)
            .FirstOrDefaultAsync();

        var update = Builders<ChatDocument>.Update
            .Set(c => c.LastMessageId, messageId)
            .Set(c => c.LastMessageAt, message?.SentAt ?? DateTime.UtcNow);

        await _context.Chats.UpdateOneAsync(
            c => c.Id == chatId,
            update);
    }

    public async Task<bool> IsUserInChatAsync(Guid chatId, string userId)
    {
        return await _context.Chats
            .Find(c => c.Id == chatId && c.Participants.Any(p => p.UserId == userId))
            .AnyAsync();
    }
}