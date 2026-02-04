using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IMessagesRepository
    {
        Task<IEnumerable<Message>> GetMessagesByChatIdAsync(Guid chatId);
        Task<Message?> GetByIdAsync(Guid messageId);
        Task AddAsync(Message message);
        Task DeleteAsync(Message message);
        Task UpdateAsync(Message message);
        Task MarkAsReadAsync(Guid messageId);
    }

}
