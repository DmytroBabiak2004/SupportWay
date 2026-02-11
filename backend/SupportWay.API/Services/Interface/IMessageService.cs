using SupportWay.Data.Models;

public interface IMessageService
{
    Task<IEnumerable<Message>> GetHistoryAsync(Guid chatId);
    Task<Message?> UpdateAsync(Guid messageId, string userId, string newContent);
    Task<bool> DeleteAsync(Guid messageId, string userId);
    Task<bool> MarkChatAsReadAsync(Guid chatId, string userId, Guid lastReadMessageId);
}