public interface IPostService
{
    Task<IEnumerable<PostDto>> GetUserPostsAsync(string userId, int page, int size);
    Task<IEnumerable<PostDto>> GetFeedPostsAsync(string userId, int page, int size);
    Task AddPostAsync(PostDto postDto);
    Task DeletePostAsync(int postId);
}
