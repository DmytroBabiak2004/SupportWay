using SupportWay.API.DTOs;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.Core.Services
{
    public class PostLikeService : IPostLikeService
    {
        private readonly IPostLikesRepoository _postLikesRepo;

        public PostLikeService(IPostLikesRepoository postLikesRepo)
        {
            _postLikesRepo = postLikesRepo;
        }

        public async Task AddLikeAsync(PostLikeDto dto)
        {
            await _postLikesRepo.AddPostLikeAsync(dto.PostId, dto.UserId);
        }

        public async Task RemoveLikeAsync(PostLikeDto dto)
        {
            await _postLikesRepo.DeletePostLikeAsync(dto.PostId, dto.UserId);
        }

        public async Task<int> GetLikesCountAsync(Guid postId)
        {
            return await _postLikesRepo.GetLikesCountAsync(postId);
        }
    }
}
