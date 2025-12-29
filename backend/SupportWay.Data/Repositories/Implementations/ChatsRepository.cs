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
            .Include(c => c.UserChats)
                .ThenInclude(uc => uc.User)
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == chatId);
    }

    public async Task<IEnumerable<Chat>> GetChatsByUserIdAsync(string userId)
    {
        return await _context.Chats
            .Include(c => c.UserChats)
                .ThenInclude(uc => uc.User)
            .Include(c => c.Messages)
            .Where(c => c.UserChats.Any(uc => uc.UserId == userId))
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
        return await _context.UserChats
            .AnyAsync(uc => uc.ChatId == chatId && uc.UserId == userId);
    }
}
