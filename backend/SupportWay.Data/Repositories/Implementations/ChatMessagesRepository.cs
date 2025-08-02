using Microsoft.EntityFrameworkCore;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

public class ChatMessageRepository : IChatMessagesRepository
{
    private readonly SupportWayContext _context;

    public ChatMessageRepository(SupportWayContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ChatMessage>> GetMessagesByChatIdAsync(int chatId)
    {
        return await _context.ChatMessages
            .Where(m => m.ChatId == chatId)
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }

    public async Task<ChatMessage?> GetByIdAsync(int messageId)
    {
        return await _context.ChatMessages.FirstOrDefaultAsync(m => m.Id == messageId);
    }

    public async Task AddAsync(ChatMessage message)
    {
        await _context.ChatMessages.AddAsync(message);
        await _context.SaveChangesAsync();
    }

    public async Task MarkAsReadAsync(int messageId)
    {
        var message = await _context.ChatMessages.FindAsync(messageId);
        if (message != null && !message.IsRead)
        {
            message.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }
}
