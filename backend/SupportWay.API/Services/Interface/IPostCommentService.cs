namespace SupportWay.API.Services.Interface
{
    public interface IPostCommentService
    {
        Task<IEnumerable<PostCommentDto>> GetCommentsByPostAsync(Guid postId);
        Task AddCommentAsync(CreatePostCommentDto dto, string userId);
        Task DeleteCommentAsync(Guid commentId, string userId);
    }

}
