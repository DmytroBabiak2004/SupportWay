using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetByUserIdAsync(
            string userId, int page, int pageSize, bool? unreadOnly = null);

        Task<int> GetUnreadCountAsync(string userId);
        Task<Notification?> GetByIdAsync(Guid id);
        Task AddAsync(Notification notification);
        void Delete(Notification notification);
        Task MarkAsReadAsync(Guid id, string userId);
        Task MarkAllAsReadAsync(string userId);
        Task SaveChangesAsync();
    }
}