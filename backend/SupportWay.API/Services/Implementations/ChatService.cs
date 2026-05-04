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
        var chats = (await _repo.GetChatsByUserIdAsync(userId)).ToList();
        var userIds = chats
            .SelectMany(c => c.UserChats.Select(uc => uc.UserId))
            .Distinct()
            .ToList();

        var users = await _db.Users
            .AsNoTracking()
            .Include(u => u.Profile)
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        return chats.Select(c => BuildListItem(c, userId, users));
    }

    public async Task<Chat?> GetByIdAsync(Guid id)
        => await _repo.GetByIdAsync(id);

    public async Task<ChatListItemDto> GetOrCreatePrivateChatAsync(string user1Id, string user2Id)
    {
        var existing = await _repo.GetPrivateChatByParticipantsAsync(user1Id, user2Id);
        if (existing != null)
        {
            var existingUsers = await LoadUsersAsync(existing.UserChats.Select(uc => uc.UserId));
            return BuildListItem(existing, user1Id, existingUsers);
        }

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

        var users = await LoadUsersAsync(new[] { user1.Id, user2.Id });
        return BuildListItem(chat, user1Id, users);
    }

    public async Task DeleteChatAsync(Guid chatId)
    {
        var chat = await _repo.GetByIdAsync(chatId);
        if (chat == null) return;

        await _repo.DeleteAsync(chat);
    }

    public async Task<bool> IsUserInChatAsync(Guid chatId, string userId)
        => await _repo.IsUserInChatAsync(chatId, userId);

    public Task<IReadOnlyList<string>> GetParticipantUserIdsAsync(Guid chatId)
        => _repo.GetParticipantUserIdsAsync(chatId);

    private async Task<Dictionary<string, User>> LoadUsersAsync(IEnumerable<string> ids)
    {
        var distinctIds = ids.Distinct().ToList();
        return await _db.Users
            .AsNoTracking()
            .Include(u => u.Profile)
            .Where(u => distinctIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);
    }

    private static ChatListItemDto BuildListItem(Chat chat, string currentUserId, IReadOnlyDictionary<string, User> users)
    {
        var otherUserId = chat.UserChats.FirstOrDefault(uc => uc.UserId != currentUserId)?.UserId;
        users.TryGetValue(otherUserId ?? string.Empty, out var other);
        var profile = other?.Profile;

        var lastMsg = chat.Messages
            .OrderByDescending(m => m.SentAt)
            .FirstOrDefault();

        var unreadCount = chat.Messages.Count(m => !m.IsRead && m.SenderId != currentUserId);

        string displayName = chat.IsPrivate
            ? (other?.UserName ?? profile?.Name ?? chat.Name)
            : chat.Name;

        return new ChatListItemDto
        {
            Id = chat.Id,
            DisplayName = displayName,
            OtherUserId = other?.Id,
            OtherUserPhotoBase64 = profile?.Photo != null
                ? Convert.ToBase64String(profile.Photo)
                : null,
            LastMessage = DescribeLastMessage(lastMsg),
            LastMessageAt = lastMsg?.SentAt,
            UnreadCount = unreadCount,
            IsPrivate = chat.IsPrivate
        };
    }

    private static string? DescribeLastMessage(Message? message)
    {
        if (message == null) return null;

        return message.MessageType switch
        {
            MessageType.SharedPost => string.IsNullOrWhiteSpace(message.Content)
                ? "Поділився постом"
                : $"Поділився постом: {message.Content}",

            MessageType.SharedHelpRequest => string.IsNullOrWhiteSpace(message.Content)
                ? "Поділився запитом допомоги"
                : $"Поділився запитом допомоги: {message.Content}",

            _ => string.IsNullOrWhiteSpace(message.Content)
                ? "Повідомлення"
                : message.Content
        };
    }
}
