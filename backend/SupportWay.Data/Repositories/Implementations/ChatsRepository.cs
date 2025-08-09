using Microsoft.EntityFrameworkCore;
using SupportWay.Data;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

public class ChatsRepository : IChatsRepository
{
    private readonly SupportWayContext _context;

    public ChatsRepository(SupportWayContext context)
    {
        _context = context;
    }

    public async Task<Chat?> GetByIdAsync(Guid chatId)
    {
        return await _context.Chats
            .Include(c => c.Users)
            .FirstOrDefaultAsync(c => c.Id == chatId);
    }

    public async Task<IEnumerable<Chat>> GetChatsByUserIdAsync(string userId)
    {
        return await _context.Chats
            .Where(c => c.Users.Any(u => u.Id == userId))
            .ToListAsync();
    }

    public async Task AddAsync(Chat chat)
    {
        await _context.Chats.AddAsync(chat);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Chat chat)
    {
        _context.Chats.Remove(chat);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsUserInChatAsync(Guid chatId, string userId)
    {
        var chat = await _context.Chats
            .Include(c => c.Users)
            .FirstOrDefaultAsync(c => c.Id == chatId);

        return chat != null && chat.Users.Any(u => u.Id == userId);
    }
}
