using SupportWay.API.DTOs;

public interface IMessageService
{
    Task<IEnumerable<MessageDto>> GetHistoryAsync(Guid chatId, string currentUserId);

    Task<MessageDto?> UpdateAsync(Guid messageId, string userId, string newContent);

    Task<bool> DeleteAsync(Guid messageId, string userId);
    Task<MessageDto?> DeleteAndReturnAsync(Guid messageId, string userId);

    Task<bool> MarkChatAsReadAsync(Guid chatId, string userId, Guid lastReadMessageId);

    Task<MessageDto> CreateTextMessageAsync(Guid chatId, string senderId, string text);

    Task<MessageDto> SharePostAsync(Guid chatId, string senderId, Guid postId, string? caption);

    Task<MessageDto> ShareHelpRequestAsync(Guid chatId, string senderId, Guid helpRequestId, string? caption);
}