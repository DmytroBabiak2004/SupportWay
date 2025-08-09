using SupportWay.API.Services.Interface;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

namespace SupportWay.API.Services.Implementations
{
    public class PostCommentService : IPostCommentService
    {
        private readonly IPostCommentsRepository _commentRepo;

        public PostCommentService(IPostCommentsRepository commentRepo)
        {
            _commentRepo = commentRepo;
        }

        public async Task<IEnumerable<PostCommentDto>> GetCommentsByPostAsync(Guid postId)
        {
            var comments = await _commentRepo.GetCommentsByPostAsync(postId);
            return comments.Select(c => new PostCommentDto
            {
                Id = c.Id,
                UserId = c.UserId,
                UserName = c.User.UserName,
                Text = c.Text,
                CreatedAt = c.CreatedAt
            });
        }

        public async Task AddCommentAsync(CreatePostCommentDto dto, string userId)
        {
            var comment = new PostComment
            {
                UserId = userId,
                PostId = dto.PostId,
                Text = dto.Text,
                CreatedAt = DateTime.UtcNow
            };

            await _commentRepo.AddCommentAsync(comment);
        }

        public async Task DeleteCommentAsync(Guid commentId, string userId)
        {
            var comment = await _commentRepo.GetCommentByIdAsync(commentId);
            if (comment == null || comment.UserId != userId)
                throw new UnauthorizedAccessException("Not allowed");

            await _commentRepo.DeleteCommentAsync(commentId);
        }
    }

}
