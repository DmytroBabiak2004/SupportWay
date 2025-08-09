using SupportWay.API.DTOs;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.Core.Services
{
    public interface IPostLikeService
    {
        public  Task AddLikeAsync(PostLikeDto dto);
        public  Task RemoveLikeAsync(PostLikeDto dto);
        public  Task<int> GetLikesCountAsync(Guid postId);
       
    }
}
