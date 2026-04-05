using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SupportWay.API.DTOs;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

public class ChatService : IChatService
{
    private readonly IChatsRepository _repo;
    private readonly SupportWayContext _db;
    private readonly UserManager<User> _userManager;

    public ChatService(IChatsRepository repo, SupportWayContext db, UserManager<User> userManager)
    {
        _repo = repo;
        _db = db;
        _userManager = userManager;
    }

    public async Task<IEnumerable<ChatListItemDto>> GetChatsByUserIdAsync(string userId)
    {
        var chats = await _db.Chats
            .Include(c => c.UserChats).ThenInclude(uc => uc.User).ThenInclude(u => u.Profile)
            .Include(c => c.Messages)
            .Where(c => c.UserChats.Any(uc => uc.UserId == userId))
            .OrderByDescending(c => c.Messages.Max(m => (DateTime?)m.SentAt) ?? c.StartedAt)
            .ToListAsync();

        return chats.Select(c => BuildListItem(c, userId));
    }

    public async Task<Chat?> GetByIdAsync(Guid id)
        => await _repo.GetByIdAsync(id);

    public async Task<ChatListItemDto> GetOrCreatePrivateChatAsync(string user1Id, string user2Id)
    {
        // Look for existing private chat between exactly these two users
        var existing = await _db.Chats
            .Include(c => c.UserChats).ThenInclude(uc => uc.User).ThenInclude(u => u.Profile)
            .Include(c => c.Messages)
            .Where(c =>
                c.IsPrivate &&
                c.UserChats.Any(uc => uc.UserId == user1Id) &&
                c.UserChats.Any(uc => uc.UserId == user2Id) &&
                c.UserChats.Count == 2)
            .FirstOrDefaultAsync();

        if (existing != null)
            return BuildListItem(existing, user1Id);

        // Create new
        var user1 = await _userManager.FindByIdAsync(user1Id)
            ?? throw new Exception("User not found");
        var user2 = await _userManager.FindByIdAsync(user2Id)
            ?? throw new Exception("User not found");

        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Name = $"{user1.UserName} & {user2.UserName}",
            StartedAt = DateTime.UtcNow,
            IsPrivate = true,
            UserChats = new List<UserChat>
            {
                new() { UserId = user1.Id },
                new() { UserId = user2.Id }
            }
        };

        await _repo.AddAsync(chat);

        // Reload with includes
        var created = await _db.Chats
            .Include(c => c.UserChats).ThenInclude(uc => uc.User).ThenInclude(u => u.Profile)
            .Include(c => c.Messages)
            .FirstAsync(c => c.Id == chat.Id);

        return BuildListItem(created, user1Id);
    }

    public async Task DeleteChatAsync(Guid chatId)
    {
        var chat = await _repo.GetByIdAsync(chatId);
        if (chat == null) return;
        await _repo.DeleteAsync(chat);
    }

    public async Task<bool> IsUserInChatAsync(Guid chatId, string userId)
        => await _repo.IsUserInChatAsync(chatId, userId);

    // ── Private ──────────────────────────────────────────────────────────────

    private static ChatListItemDto BuildListItem(Chat chat, string currentUserId)
    {
        var otherUC = chat.UserChats.FirstOrDefault(uc => uc.UserId != currentUserId);
        var other = otherUC?.User;
        var profile = other?.Profile;

        var lastMsg = chat.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
        var unreadCount = chat.Messages.Count(m => !m.IsRead && m.SenderId != currentUserId);

        // ЗМІНА ТУТ: Пріоритет віддаємо іншому користувачу (UserName)
        string displayName = chat.IsPrivate
            ? (other?.UserName ?? profile?.Name ?? chat.Name)
            : chat.Name;

        return new ChatListItemDto
        {
            Id = chat.Id,
            DisplayName = displayName, // Тепер тут буде username співрозмовника
            OtherUserId = other?.Id,
            OtherUserPhotoBase64 = profile?.Photo != null
                                   ? Convert.ToBase64String(profile.Photo)
                                   : null,
            LastMessage = lastMsg?.Content,
            LastMessageAt = lastMsg?.SentAt,
            UnreadCount = unreadCount,
            IsPrivate = chat.IsPrivate
        };
    }
}
