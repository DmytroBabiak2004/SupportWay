using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportWay.API.DTOs;
using SupportWay.Core.Services;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly IPostLikeService _likeService;

    public PostsController(IPostService postService, IPostLikeService likeService)
    {
        _postService = postService;
        _likeService = likeService;
    }

    [HttpGet("user")]
    public async Task<IActionResult> GetMyPosts([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var posts = await _postService.GetUserPostsAsync(userId, page, size);
        return Ok(posts);
    }

    [HttpGet("feed")]
    public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var posts = await _postService.GetFeedPostsAsync(userId, page, size);
        return Ok(posts);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePost([FromForm] CreatePostRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var dto = new PostDto
        {
            Title = request.Title,
            Content = request.Content,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        if (request.Image != null)
        {
            using var memoryStream = new MemoryStream();
            await request.Image.CopyToAsync(memoryStream);
            dto.Image = memoryStream.ToArray();
        }

        await _postService.AddPostAsync(dto);
        return StatusCode(201);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        await _postService.DeletePostAsync(id);
        return NoContent();
    }

    // --- ЛАЙКИ ---

    [HttpPost("{postId}/like")]
    public async Task<IActionResult> LikePost(Guid postId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        await _likeService.AddLikeAsync(new PostLikeDto { PostId = postId, UserId = userId });
        return Ok();
    }

    [HttpDelete("{postId}/like")]
    public async Task<IActionResult> UnlikePost(Guid postId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        await _likeService.RemoveLikeAsync(new PostLikeDto { PostId = postId, UserId = userId });
        return Ok();
    }

    [HttpGet("{postId}/likes")]
    public async Task<IActionResult> GetLikesCount(Guid postId)
    {
        var count = await _likeService.GetLikesCountAsync(postId);
        return Ok(count);
    }
}