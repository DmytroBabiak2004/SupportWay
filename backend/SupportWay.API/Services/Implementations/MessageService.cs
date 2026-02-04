using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

public class MessageService : IMessageService
{
    private readonly IMessagesRepository _repo;

    public MessageService(IMessagesRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<Message>> GetHistoryAsync(Guid chatId)
    {
        return await _repo.GetMessagesByChatIdAsync(chatId);
    }

    public async Task<Message?> UpdateAsync(Guid messageId, string userId, string newContent)
    {
        var message = await _repo.GetByIdAsync(messageId);

        if (message == null || message.SenderId != userId)
            return null;

        message.Content = newContent;
       
        await _repo.UpdateAsync(message);
        return message;
    }

    public async Task<bool> DeleteAsync(Guid messageId, string userId)
    {
        var message = await _repo.GetByIdAsync(messageId);

        if (message == null || message.SenderId != userId)
            return false;

        await _repo.DeleteAsync(message);
        return true;
    }

    public async Task<bool> MarkAsReadAsync(Guid messageId)
    {
        var message = await _repo.GetByIdAsync(messageId);
        if (message == null) return false;

        await _repo.MarkAsReadAsync(messageId);
        return true;
    }
}