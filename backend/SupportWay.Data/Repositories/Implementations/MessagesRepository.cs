using Microsoft.EntityFrameworkCore;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

public class ChatMessageRepository : IMessagesRepository
{
    private readonly SupportWayContext _context;

    public ChatMessageRepository(SupportWayContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Message>> GetMessagesByChatIdAsync(Guid chatId)
    {
        return await _context.Messages
            .Where(m => m.ChatId == chatId)
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }

    public async Task<Message?> GetByIdAsync(Guid messageId)
    {
        return await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId);
    }

    public async Task AddAsync(Message message)
    {
        await _context.Messages.AddAsync(message);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Message message)
    {
        _context.Messages.Update(message);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Message message)
    {
        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();
    }

    public async Task MarkAsReadAsync(Guid messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message != null && !message.IsRead)
        {
            message.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }
    public Task<bool> IsUserInChatAsync(Guid chatId, string userId)
    {
        return _context.UserChats.AnyAsync(uc => uc.ChatId == chatId && uc.UserId == userId);
    }

    public async Task MarkChatAsReadUpToAsync(Guid chatId, string userId, DateTime upToSentAt)
    {
   
        await _context.Messages
            .Where(m =>
                m.ChatId == chatId &&
                !m.IsRead &&
                m.SentAt <= upToSentAt &&
                m.SenderId != userId 
            )
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(m => m.IsRead, true)
            );
    }
}
