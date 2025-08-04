using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportWay.Data.Models;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PostsController : ControllerBase
{
    private readonly IPostService _service;

    public PostsController(IPostService service)
    {
        _service = service;
    }

    [HttpGet("user")]
    public async Task<IActionResult> GetMyPosts([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var posts = await _service.GetUserPostsAsync(userId, page, size);
        return Ok(posts);
    }

    [HttpGet("feed")]
    public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var posts = await _service.GetFeedPostsAsync(userId, page, size);
        return Ok(posts);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePost([FromBody] PostDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        dto.UserId = userId;
        await _service.AddPostAsync(dto);
        return StatusCode(201);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(int id)
    {
        await _service.DeletePostAsync(id);
        return NoContent();
    }
}
