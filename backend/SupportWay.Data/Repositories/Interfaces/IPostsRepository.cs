using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IPostRepository
    {
        Task<IEnumerable<Post>> GetPostsByFollowedUsersAsync(string currentUserId, int pageNumber, int pageSize);
        Task<IEnumerable<Post>> GetPostByUserAsync(string userId, int pageNumber, int pageSize);
        Task<Post?> GetPostByIdAsync(string postId);
        Task AddPostAsync (Post post);
        Task UpdatePostAsync (Post post);
        Task DeletePostAsync (int postId);
    }
}
