using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IPostLikesRepoository
    {
        Task<int> GetLikesCountAsync(Guid postId);
        Task<bool> HasUserLikedPostAsync(Guid postId, string userId);
        Task AddPostLikeAsync(Guid postId, string userId);
        Task DeletePostLikeAsync(Guid postId, string userId);
    }
}
