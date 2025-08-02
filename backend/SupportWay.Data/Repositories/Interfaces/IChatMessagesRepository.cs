using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IChatMessagesRepository
    {
        Task<IEnumerable<ChatMessage>> GetMessagesByChatIdAsync(int chatId);
        Task<ChatMessage?> GetByIdAsync(int messageId);
        Task AddAsync(ChatMessage message);
        Task MarkAsReadAsync(int messageId);
    }

}
