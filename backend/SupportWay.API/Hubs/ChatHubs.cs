using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SupportWay.API.DTOs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly IMessageService _messageService;

    public ChatHub(
        IChatService chatService,
        IMessageService messageService)
    {
        _chatService = chatService;
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
        if (string.IsNullOrEmpty(fromUserId))
            return;

        if (!Guid.TryParse(chatId, out var chatGuid))
            throw new HubException("Invalid chatId");

        var users = await _chatService.GetParticipantUserIdsAsync(chatGuid);

        foreach (var userId in users.Where(x => x != fromUserId))
        {
            await Clients.User(userId)
                .SendAsync("typing", fromUserId, chatId);
        }
    }

    public async Task Seen(string chatId, Guid lastReadMessageId)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId))
            throw new HubException("Unauthorized");

        if (!Guid.TryParse(chatId, out var chatGuid))
            throw new HubException("Invalid chatId");

        var success = await _messageService.MarkChatAsReadAsync(chatGuid, userId, lastReadMessageId);
        if (!success)
            throw new HubException("Access denied");

        var users = await _chatService.GetParticipantUserIdsAsync(chatGuid);

        foreach (var u in users.Where(x => x != userId))
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

        var deletedMessage = await _messageService.DeleteAndReturnAsync(messageId, userId);
        if (deletedMessage is null)
            throw new HubException("Forbidden");

        var users = await _chatService.GetParticipantUserIdsAsync(deletedMessage.ChatId);

        foreach (var u in users)
        {
            await Clients.User(u).SendAsync("messageDeleted", new
            {
                chatId = deletedMessage.ChatId.ToString(),
                messageId,
                deletedBy = userId
            });
        }
    }
    private async Task BroadcastMessageToChatUsers(Guid chatId, MessageDto dto)
    {
        var users = await _chatService.GetParticipantUserIdsAsync(chatId);

        foreach (var userId in users)
        {
            await Clients.User(userId).SendAsync("receiveMessage", dto);
        }
    }
}