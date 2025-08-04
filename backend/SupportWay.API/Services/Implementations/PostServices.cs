using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

public class PostService : IPostService
{
    private readonly IPostRepository _repo;

    public PostService(IPostRepository repo)
    {
        _repo = repo;
    }

    public async Task AddPostAsync(PostDto dto)
    {
        var post = new Post
        {
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow,
            UserId = dto.UserId
        };
        await _repo.AddPostAsync(post);
    }

    public async Task DeletePostAsync(int postId)
    {
        await _repo.DeletePostAsync(postId);
    }

    public async Task<IEnumerable<PostDto>> GetUserPostsAsync(string userId, int page, int size)
    {
        var posts = await _repo.GetPostByUserAsync(userId, page, size);
        return posts.Select(p => new PostDto
        {
            Id = p.Id,
            Content = p.Content,
            CreatedAt = p.CreatedAt,
            UserId = p.UserId
        });
    }

    public async Task<IEnumerable<PostDto>> GetFeedPostsAsync(string userId, int page, int size)
    {
        var posts = await _repo.GetPostsByFollowedUsersAsync(userId, page, size);
        return posts.Select(p => new PostDto
        {
            Id = p.Id,
            Content = p.Content,
            CreatedAt = p.CreatedAt,
            UserId = p.UserId
        });
    }
}
