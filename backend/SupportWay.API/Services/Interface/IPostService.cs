using SupportWay.API.DTOs;
public interface IPostService
{
    Task<IEnumerable<PostDto>> GetUserPostsAsync(string userId, int page, int size);
    Task<IEnumerable<PostDto>> GetFeedPostsAsync(string userId, int page, int size);
    Task AddPostAsync(PostDto postDto);
    Task DeletePostAsync(Guid postId);
}
