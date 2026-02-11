using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

[Authorize]
public class ChatHub : Hub

{
    private readonly IMessagesRepository _messageRepo;
    private readonly SupportWayContext _context;

    public ChatHub(
        IMessagesRepository messageRepo,
        SupportWayContext context)
    {
        _messageRepo = messageRepo;
        _context = context;
    }

    /// <summary>
    /// Підключення користувача.
    /// UserId береться з Identity (JWT / Cookie).
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;

        if (string.IsNullOrEmpty(userId))
            throw new HubException("Unauthorized");

        Console.WriteLine("USER ID: " + Context.UserIdentifier);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Відправка повідомлення в чат
    /// </summary>
    public async Task SendMessage(string chatId, string text)
    {
        var fromUserId = Context.UserIdentifier;
        if (fromUserId is null)
            throw new HubException("Unauthorized");

        Guid chatGuid = Guid.Parse(chatId);

        bool isMember = await _context.UserChats
            .AnyAsync(x => x.ChatId == chatGuid && x.UserId == fromUserId);

        if (!isMember)
            throw new HubException("Access denied");

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChatId = chatGuid,
            SenderId = fromUserId,
            Content = text,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        await _messageRepo.AddAsync(message);

        // Отримуємо всіх учасників чату
        var users = await _context.UserChats
            .Where(x => x.ChatId == chatGuid)
            .Select(x => x.UserId)
            .ToListAsync();

        // Надсилаємо кожному користувачу
        foreach (var userId in users)
        {
            await Clients.User(userId).SendAsync(
                "receiveMessage",
                message.Id,
                fromUserId,
                text,
                chatId,
                message.SentAt.ToString("O")
            );
        }
    }

    /// <summary>
    /// Індикатор набору тексту
    /// </summary>
    public async Task Typing(string chatId)
    {
        var fromUserId = Context.UserIdentifier;
        if (fromUserId is null) return;

        var users = await _context.UserChats
            .Where(x => x.ChatId == Guid.Parse(chatId) && x.UserId != fromUserId)
            .Select(x => x.UserId)
            .ToListAsync();

        foreach (var userId in users)
        {
            await Clients.User(userId)
                .SendAsync("typing", fromUserId, chatId);
        }
    }

    /// <summary>
    /// Повідомлення прочитано
    /// </summary>
    public async Task Seen(string chatId, Guid lastReadMessageId)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId)) return;

        var chatGuid = Guid.Parse(chatId);

        // 1) membership
        var isMember = await _context.UserChats
            .AnyAsync(x => x.ChatId == chatGuid && x.UserId == userId);
        if (!isMember) throw new HubException("Access denied");

        // 2) беремо last message
        var last = await _context.Messages.FirstOrDefaultAsync(m => m.Id == lastReadMessageId);
        if (last == null || last.ChatId != chatGuid) return;

        // 3) bulk read до last.SentAt тільки для чужих
        await _context.Messages
            .Where(m =>
                m.ChatId == chatGuid &&
                !m.IsRead &&
                m.SentAt <= last.SentAt &&
                m.SenderId != userId
            )
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));

        // 4) повідомляємо інших
        var users = await _context.UserChats
            .Where(x => x.ChatId == chatGuid && x.UserId != userId)
            .Select(x => x.UserId)
            .ToListAsync();

        foreach (var u in users)
        {
            await Clients.User(u).SendAsync("chatSeen", new
            {
                chatId = chatId,
                userId = userId,
                lastReadMessageId = lastReadMessageId
            });
        }
    }
    /// <summary>
/// Видалення повідомлення (тільки автор може видаляти)
/// </summary>
public async Task DeleteMessage(Guid messageId)
{
    var userId = Context.UserIdentifier;
    if (string.IsNullOrEmpty(userId))
        throw new HubException("Unauthorized");

    // 1) Дістаємо повідомлення
    var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId);
    if (message is null)
        throw new HubException("Message not found");

    // 2) Тільки автор може видалити
    if (message.SenderId != userId)
        throw new HubException("Forbidden");

    // 3) Перевіряємо, що юзер учасник чату (на всяк випадок)
    var isMember = await _context.UserChats
        .AnyAsync(x => x.ChatId == message.ChatId && x.UserId == userId);
    if (!isMember)
        throw new HubException("Access denied");

    // 4) Видаляємо
    _context.Messages.Remove(message);
    await _context.SaveChangesAsync();

    // 5) Розсилаємо івент всім учасникам
    var users = await _context.UserChats
        .Where(x => x.ChatId == message.ChatId)
        .Select(x => x.UserId)
        .ToListAsync();

    foreach (var u in users)
    {
        await Clients.User(u).SendAsync("messageDeleted", new
        {
            chatId = message.ChatId.ToString(),
            messageId = messageId,
            deletedBy = userId
        });
    }
}


}
