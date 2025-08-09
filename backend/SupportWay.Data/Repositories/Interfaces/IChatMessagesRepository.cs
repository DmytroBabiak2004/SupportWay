using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IChatMessagesRepository
    {
        Task<IEnumerable<ChatMessage>> GetMessagesByChatIdAsync(Guid chatId);
        Task<ChatMessage?> GetByIdAsync(Guid messageId);
        Task AddAsync(ChatMessage message);
        Task MarkAsReadAsync(Guid messageId);
    }

}
