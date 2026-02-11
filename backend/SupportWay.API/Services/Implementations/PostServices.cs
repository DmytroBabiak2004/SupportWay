using SupportWay.API.DTOs;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class PostService : IPostService
{
    private readonly IPostRepository _postsRepo;
    private readonly IPostLikesRepoository _likesRepo;
    private readonly IPostCommentsRepository _commentsRepo;

    public PostService(
        IPostRepository postsRepo,
        IPostLikesRepoository likesRepo,
        IPostCommentsRepository commentsRepo)
    {
        _postsRepo = postsRepo;
        _likesRepo = likesRepo;
        _commentsRepo = commentsRepo;
    }

    public async Task AddPostAsync(PostDto dto)
    {
        var post = new Post
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Content = dto.Content,
            Image = dto.Image,
            CreatedAt = DateTime.UtcNow,
            UserId = dto.UserId
        };
        await _postsRepo.AddPostAsync(post);
    }

    public async Task DeletePostAsync(Guid postId)
    {
        await _postsRepo.DeletePostAsync(postId);
    }

    public async Task<IEnumerable<PostDto>> GetUserPostsAsync(string currentUserId, int page, int size)
    {
        var posts = await _postsRepo.GetPostByUserAsync(currentUserId, page, size);
        return await BuildPostDtosAsync(posts, currentUserId);
    }

    public async Task<IEnumerable<PostDto>> GetFeedPostsAsync(string currentUserId, int page, int size)
    {
        var posts = await _postsRepo.GetPostsByFollowedUsersAsync(currentUserId, page, size);
        return await BuildPostDtosAsync(posts, currentUserId);
    }

    // 🔥 Основний метод для формування PostDto з лайками та коментарями
    private async Task<IEnumerable<PostDto>> BuildPostDtosAsync(IEnumerable<Post> posts, string currentUserId)
    {
        var result = new List<PostDto>();

        foreach (var post in posts)
        {
            // Кількість лайків
            var likesCount = await _likesRepo.GetLikesCountAsync(post.Id);

            // Перевіряємо, чи поточний користувач лайкнув пост
            var isLiked = await _likesRepo.HasUserLikedPostAsync(post.Id, currentUserId);

            // Кількість коментарів
            var commentsCount = (await _commentsRepo.GetCommentsByPostAsync(post.Id)).Count();

            result.Add(new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                Image = post.Image,
                CreatedAt = post.CreatedAt,
                UserId = post.UserId,

                // Дані автора
                AuthorUserName = post.User?.UserName,
                AuthorFullName = post.User?.Profile?.FullName,
                AuthorPhotoBase64 = post.User?.Profile?.Photo,

                LikesCount = likesCount,
                CommentsCount = commentsCount,
                IsLikedByCurrentUser = isLiked
            });
        }

        return result;
    }
}
