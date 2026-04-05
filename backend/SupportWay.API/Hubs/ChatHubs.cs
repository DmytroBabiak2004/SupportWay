using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SupportWay.API.DTOs;
using SupportWay.Data.Context;

[Authorize]
public class ChatHub : Hub
{
    private readonly SupportWayContext _context;
    private readonly IMessageService _messageService;

    public ChatHub(
        SupportWayContext context,
        IMessageService messageService)
    {
        _context = context;
        _messageService = messageService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;

        if (string.IsNullOrEmpty(userId))
            throw new HubException("Unauthorized");

        await base.OnConnectedAsync();
    }

    public async Task SendMessage(string chatId, string text)
    {
        var fromUserId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(fromUserId))
            throw new HubException("Unauthorized");

        if (!Guid.TryParse(chatId, out var chatGuid))
            throw new HubException("Invalid chatId");

        var dto = await _messageService.CreateTextMessageAsync(chatGuid, fromUserId, text);
        await BroadcastMessageToChatUsers(chatGuid, dto);
    }

    public async Task SharePost(string chatId, string postId, string? caption)
    {
        var fromUserId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(fromUserId))
            throw new HubException("Unauthorized");

        if (!Guid.TryParse(chatId, out var chatGuid))
            throw new HubException("Invalid chatId");

        if (!Guid.TryParse(postId, out var postGuid))
            throw new HubException("Invalid postId");

        var dto = await _messageService.SharePostAsync(chatGuid, fromUserId, postGuid, caption);
        await BroadcastMessageToChatUsers(chatGuid, dto);
    }

    public async Task ShareHelpRequest(string chatId, string helpRequestId, string? caption)
    {
        var fromUserId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(fromUserId))
            throw new HubException("Unauthorized");

        if (!Guid.TryParse(chatId, out var chatGuid))
            throw new HubException("Invalid chatId");

        if (!Guid.TryParse(helpRequestId, out var requestGuid))
            throw new HubException("Invalid helpRequestId");

        var dto = await _messageService.ShareHelpRequestAsync(chatGuid, fromUserId, requestGuid, caption);
        await BroadcastMessageToChatUsers(chatGuid, dto);
    }

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

    public async Task Seen(string chatId, Guid lastReadMessageId)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId)) return;

        var chatGuid = Guid.Parse(chatId);

        var isMember = await _context.UserChats
            .AnyAsync(x => x.ChatId == chatGuid && x.UserId == userId);
        if (!isMember) throw new HubException("Access denied");

        var last = await _context.Messages.FirstOrDefaultAsync(m => m.Id == lastReadMessageId);
        if (last == null || last.ChatId != chatGuid) return;

        await _context.Messages
            .Where(m =>
                m.ChatId == chatGuid &&
                !m.IsRead &&
                m.SentAt <= last.SentAt &&
                m.SenderId != userId
            )
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));

        var users = await _context.UserChats
            .Where(x => x.ChatId == chatGuid && x.UserId != userId)
            .Select(x => x.UserId)
            .ToListAsync();

        foreach (var u in users)
        {
            await Clients.User(u).SendAsync("chatSeen", new
            {
                chatId,
                userId,
                lastReadMessageId
            });
        }
    }

    public async Task DeleteMessage(Guid messageId)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId))
            throw new HubException("Unauthorized");

        var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId);
        if (message is null)
            throw new HubException("Message not found");

        if (message.SenderId != userId)
            throw new HubException("Forbidden");

        var isMember = await _context.UserChats
            .AnyAsync(x => x.ChatId == message.ChatId && x.UserId == userId);
        if (!isMember)
            throw new HubException("Access denied");

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();

        var users = await _context.UserChats
            .Where(x => x.ChatId == message.ChatId)
            .Select(x => x.UserId)
            .ToListAsync();

        foreach (var u in users)
        {
            await Clients.User(u).SendAsync("messageDeleted", new
            {
                chatId = message.ChatId.ToString(),
                messageId,
                deletedBy = userId
            });
        }
    }

    private async Task BroadcastMessageToChatUsers(Guid chatId, MessageDto dto)
    {
        var users = await _context.UserChats
            .Where(x => x.ChatId == chatId)
            .Select(x => x.UserId)
            .ToListAsync();

        foreach (var userId in users)
        {
            await Clients.User(userId).SendAsync("receiveMessage", dto);
        }
    }
}