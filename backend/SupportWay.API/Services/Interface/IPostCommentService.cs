namespace SupportWay.API.Services.Interface
{
    public interface IPostCommentService
    {
        Task<IEnumerable<PostCommentDto>> GetCommentsByPostAsync(int postId);
        Task<IEnumerable<PostCommentDto>> GetCommentsByRequestAsync(int requestId);
        Task AddCommentAsync(CreatePostCommentDto dto, string userId);
        Task DeleteCommentAsync(int commentId, string userId);
    }

}
