using Microsoft.EntityFrameworkCore;
using SupportWay.API.DTOs;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Mongo.Documents;
using SupportWay.Data.Repositories.Interfaces;

public class MessageService : IMessageService
{
    private readonly IMongoMessagesRepository _messagesRepository;
    private readonly IChatsRepository _chatsRepository;
    private readonly SupportWayContext _db;

    public MessageService(
        IMongoMessagesRepository messagesRepository,
        IChatsRepository chatsRepository,
        SupportWayContext db)
    {
        _messagesRepository = messagesRepository;
        _chatsRepository = chatsRepository;
        _db = db;
    }

    public async Task<IEnumerable<MessageDto>> GetHistoryAsync(Guid chatId, string currentUserId)
    {
        var isUserInChat = await _chatsRepository.IsUserInChatAsync(chatId, currentUserId);
        if (!isUserInChat)
            return Enumerable.Empty<MessageDto>();

        var messages = await _messagesRepository.GetChatMessagesAsync(chatId, limit: 200);
        return await BuildDtosAsync(messages);
    }

    public async Task<MessageDto?> UpdateAsync(Guid messageId, string userId, string newContent)
    {
        var message = await _messagesRepository.GetByIdAsync(messageId);

        if (message == null || message.IsDeleted || message.SenderId != userId)
            return null;

        var isUserInChat = await _chatsRepository.IsUserInChatAsync(message.ChatId, userId);
        if (!isUserInChat)
            return null;

        message.Content = newContent.Trim();
        message.EditedAt = DateTime.UtcNow;

        await _messagesRepository.UpdateAsync(message);

        return (await BuildDtosAsync(new List<MessageDocument> { message })).FirstOrDefault();
    }

    public async Task<bool> DeleteAsync(Guid messageId, string userId)
    {
        var deleted = await DeleteAndReturnAsync(messageId, userId);
        return deleted != null;
    }

    public async Task<MessageDto?> DeleteAndReturnAsync(Guid messageId, string userId)
    {
        var message = await _messagesRepository.GetByIdAsync(messageId);

        if (message == null || message.IsDeleted || message.SenderId != userId)
            return null;

        var isUserInChat = await _chatsRepository.IsUserInChatAsync(message.ChatId, userId);
        if (!isUserInChat)
            return null;

        var dto = (await BuildDtosAsync(new List<MessageDocument> { message })).FirstOrDefault();

        await _messagesRepository.DeleteMessageAsync(message.Id);

        return dto;
    }

    public async Task<bool> MarkChatAsReadAsync(Guid chatId, string userId, Guid lastReadMessageId)
    {
        var lastMessage = await _messagesRepository.GetByIdAsync(lastReadMessageId);

        if (lastMessage == null || lastMessage.ChatId != chatId)
            return false;

        var isUserInChat = await _chatsRepository.IsUserInChatAsync(chatId, userId);
        if (!isUserInChat)
            return false;

        await _messagesRepository.MarkChatAsReadUpToAsync(chatId, userId, lastMessage.SentAt);
        return true;
    }

    public async Task<MessageDto> CreateTextMessageAsync(Guid chatId, string senderId, string text)
    {
        var isUserInChat = await _chatsRepository.IsUserInChatAsync(chatId, senderId);
        if (!isUserInChat)
            throw new InvalidOperationException("Access denied");

        var message = new MessageDocument
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            SenderId = senderId,
            Content = text.Trim(),
            SentAt = DateTime.UtcNow,
            IsRead = false,
            IsDeleted = false,
            MessageType = MessageType.Text
        };

        await _messagesRepository.CreateMessageAsync(message);
        return (await BuildDtosAsync(new List<MessageDocument> { message })).First();
    }

    public async Task<MessageDto> SharePostAsync(Guid chatId, string senderId, Guid postId, string? caption)
    {
        var isUserInChat = await _chatsRepository.IsUserInChatAsync(chatId, senderId);
        if (!isUserInChat)
            throw new InvalidOperationException("Access denied");

        var anyEntity = await _db.Posts
            .AsNoTracking()
            .Where(p => p.Id == postId)
            .Select(p => new
            {
                p.Id,
                PostType = EF.Property<string>(p, "PostType")
            })
            .FirstOrDefaultAsync();

        if (anyEntity == null)
            throw new InvalidOperationException($"Entity not found at all. Id={postId}");

        if (anyEntity.PostType == "HelpRequest")
            throw new InvalidOperationException(
                $"This id belongs to HelpRequest, not Post. Id={postId}. " +
                $"Frontend is calling SharePost instead of ShareHelpRequest."
            );

        var post = await _db.Posts
            .AsNoTracking()
            .Include(p => p.User)
                .ThenInclude(u => u.Profile)
            .FirstOrDefaultAsync(p =>
                p.Id == postId &&
                EF.Property<string>(p, "PostType") == "Post");

        if (post == null)
            throw new InvalidOperationException($"Post not found. Id={postId}");

        var message = new MessageDocument
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            SenderId = senderId,
            Content = caption?.Trim() ?? string.Empty,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            IsDeleted = false,
            MessageType = MessageType.SharedPost,
            SharedPostId = postId,
            SharedHelpRequestId = null
        };

        await _messagesRepository.CreateMessageAsync(message);
        return (await BuildDtosAsync(new List<MessageDocument> { message })).First();
    }

    public async Task<MessageDto> ShareHelpRequestAsync(Guid chatId, string senderId, Guid helpRequestId, string? caption)
    {
        var isUserInChat = await _chatsRepository.IsUserInChatAsync(chatId, senderId);
        if (!isUserInChat)
            throw new InvalidOperationException("Access denied");

        var request = await _db.HelpRequests
            .AsNoTracking()
            .Include(r => r.User)
                .ThenInclude(u => u.Profile)
            .FirstOrDefaultAsync(r => r.Id == helpRequestId);

        if (request == null)
            throw new InvalidOperationException("Help request not found");

        var message = new MessageDocument
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            SenderId = senderId,
            Content = caption?.Trim() ?? string.Empty,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            IsDeleted = false,
            MessageType = MessageType.SharedHelpRequest,
            SharedHelpRequestId = helpRequestId,
            SharedPostId = null
        };

        await _messagesRepository.CreateMessageAsync(message);
        return (await BuildDtosAsync(new List<MessageDocument> { message })).First();
    }

    private async Task<List<MessageDto>> BuildDtosAsync(IEnumerable<MessageDocument> messages)
    {
        var messageList = messages
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.SentAt)
            .ToList();

        var postIds = messageList
            .Where(m => m.MessageType == MessageType.SharedPost && m.SharedPostId.HasValue)
            .Select(m => m.SharedPostId!.Value)
            .Distinct()
            .ToList();

        var helpRequestIds = messageList
            .Where(m => m.MessageType == MessageType.SharedHelpRequest && m.SharedHelpRequestId.HasValue)
            .Select(m => m.SharedHelpRequestId!.Value)
            .Distinct()
            .ToList();

        var posts = postIds.Count == 0
            ? new Dictionary<Guid, Post>()
            : await _db.Posts
                .AsNoTracking()
                .Include(p => p.User)
                    .ThenInclude(u => u.Profile)
                .Where(p =>
                    postIds.Contains(p.Id) &&
                    EF.Property<string>(p, "PostType") == "Post")
                .ToDictionaryAsync(p => p.Id);

        var requests = helpRequestIds.Count == 0
            ? new Dictionary<Guid, HelpRequest>()
            : await _db.HelpRequests
                .AsNoTracking()
                .Include(r => r.User)
                    .ThenInclude(u => u.Profile)
                .Where(r => helpRequestIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id);

        var result = new List<MessageDto>(messageList.Count);

        foreach (var message in messageList)
        {
            var dto = new MessageDto
            {
                Id = message.Id,
                ChatId = message.ChatId,
                SenderId = message.SenderId,
                Content = message.Content,
                SentAt = message.SentAt,
                IsRead = message.IsRead || message.ReadByUserIds.Count > 0,
                MessageType = message.MessageType,
                SharedPostId = message.SharedPostId,
                SharedHelpRequestId = message.SharedHelpRequestId
            };

            if (message.MessageType == MessageType.SharedPost &&
                message.SharedPostId.HasValue &&
                posts.TryGetValue(message.SharedPostId.Value, out var post))
            {
                dto.SharedPreview = BuildPostPreview(post);
            }
            else if (message.MessageType == MessageType.SharedHelpRequest &&
                     message.SharedHelpRequestId.HasValue &&
                     requests.TryGetValue(message.SharedHelpRequestId.Value, out var request))
            {
                dto.SharedPreview = BuildHelpRequestPreview(request);
            }

            result.Add(dto);
        }

        return result;
    }

    private static SharedPreviewDto BuildPostPreview(Post post)
    {
        string? authorPhotoBase64 = null;
        if (post.User?.Profile?.Photo is { Length: > 0 } authorPhoto)
            authorPhotoBase64 = Convert.ToBase64String(authorPhoto);

        string? imageBase64 = null;
        if (post.Image is { Length: > 0 } img)
            imageBase64 = Convert.ToBase64String(img);

        return new SharedPreviewDto
        {
            Id = post.Id,
            EntityType = "post",
            AuthorUserName = post.User?.UserName ?? string.Empty,
            AuthorPhotoBase64 = authorPhotoBase64,
            Title = null,
            Content = post.Content ?? string.Empty,
            ImageBase64 = imageBase64,
            CreatedAt = post.CreatedAt
        };
    }

    private static SharedPreviewDto BuildHelpRequestPreview(HelpRequest request)
    {
        string? authorPhotoBase64 = null;
        if (request.User?.Profile?.Photo is { Length: > 0 } authorPhoto)
            authorPhotoBase64 = Convert.ToBase64String(authorPhoto);

        string? imageBase64 = null;
        if (request.Image is { Length: > 0 } img)
            imageBase64 = Convert.ToBase64String(img);

        return new SharedPreviewDto
        {
            Id = request.Id,
            EntityType = "helpRequest",
            AuthorUserName = request.User?.UserName ?? string.Empty,
            AuthorPhotoBase64 = authorPhotoBase64,
            Title = null,
            Content = request.Content ?? string.Empty,
            ImageBase64 = imageBase64,
            CreatedAt = request.CreatedAt
        };
    }
}
