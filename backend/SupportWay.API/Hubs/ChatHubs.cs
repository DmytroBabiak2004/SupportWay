using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

public class ChatHub : Hub
{
    private static Dictionary<string, string> Users = new();

    private readonly IChatMessagesRepository _messageRepo;
    private readonly SupportWayContext _context;

    public ChatHub(
        IChatMessagesRepository messageRepo,
        SupportWayContext context)
    {
        _messageRepo = messageRepo;
        _context = context;
    }

    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? ex)
    {
        var entry = Users.FirstOrDefault(x => x.Value == Context.ConnectionId);
        if (!string.IsNullOrEmpty(entry.Key))
            Users.Remove(entry.Key);

        return base.OnDisconnectedAsync(ex);
    }

    public Task RegisterUser(string userId)
    {
        Users[userId] = Context.ConnectionId;
        return Task.CompletedTask;
    }

    public async Task SendMessage(string chatId, string fromUserId, string text)
    {
        Guid chatGuid = Guid.Parse(chatId);

        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            ChatId = chatGuid,
            UserId = fromUserId,
            MessageText = text,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            MessageTypeId = Guid.Empty
        };

        await _messageRepo.AddAsync(message);

        var chatUsers = await _context.UserChats
            .Where(uc => uc.ChatId == chatGuid)
            .Select(uc => uc.UserId)
            .ToListAsync();


        foreach (var userId in chatUsers)
        {
            if (Users.TryGetValue(userId, out var connId))
            {
                await Clients.Client(connId).SendAsync(
                    "receiveMessage",
                    fromUserId,
                    text,
                    chatId,
                    message.SentAt.ToString("O")
                );
            }
        }
    }

    public async Task Typing(string fromUserId, string toUserId)
    {
        if (Users.TryGetValue(toUserId, out string conn))
            await Clients.Client(conn).SendAsync("typing", fromUserId);
    }

    public async Task Seen(string messageId, string userId)
    {
        await Clients.All.SendAsync("messageSeen", messageId, userId);
    }
}
