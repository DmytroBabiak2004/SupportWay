using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IPostLikesRepoository
    {
        Task<int> GetLikesCountAsync(int postId);
        Task AddPostLikeAsync(int postId, string userId);
        Task DeletePostLikeAsync(int postId, string userId);
    }
}
