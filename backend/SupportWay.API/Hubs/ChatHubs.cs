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

        // 🔒 Перевіряємо, що користувач є учасником чату
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
    public async Task Seen(Guid messageId)
    {
        var userId = Context.UserIdentifier;
        if (userId is null) return;

        var message = await _context.Messages.FindAsync(messageId);
        if (message is null) return;

        message.IsRead = true;
        await _context.SaveChangesAsync();

        var users = await _context.UserChats
            .Where(x => x.ChatId == message.ChatId)
            .Select(x => x.UserId)
            .ToListAsync();

        foreach (var u in users)
        {
            await Clients.User(u)
                .SendAsync("messageSeen", messageId, userId);
        }
    }
}
