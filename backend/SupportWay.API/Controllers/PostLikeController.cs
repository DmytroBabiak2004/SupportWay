using Microsoft.AspNetCore.Mvc;
using SupportWay.API.DTOs;
using SupportWay.Core.Services;
using SupportWay.Services;

namespace SupportWay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostLikesController : ControllerBase
    {
        private readonly IPostLikeService _postLikeService;

        public PostLikesController(IPostLikeService postLikeService)
        {
            _postLikeService = postLikeService;
        }

        [HttpPost("like")]
        public async Task<IActionResult> LikePost([FromBody] PostLikeDto dto)
        {
            var userId = User.FindFirst("sub")?.Value
                         ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found in token claims" });
            }

            dto.UserId = userId;

            try
            {
                await _postLikeService.AddLikeAsync(dto);
                return Ok(new { message = "Liked" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("unlike")]
        public async Task<IActionResult> UnlikePost([FromBody] PostLikeDto dto)
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            dto.UserId = userId;

            await _postLikeService.RemoveLikeAsync(dto);
            return Ok(new { message = "Unliked" });
        }

        [HttpGet("count/{postId}")]
        public async Task<IActionResult> GetLikesCount(Guid postId)
        {
            var count = await _postLikeService.GetLikesCountAsync(postId);
            return Ok(count);
        }
    }
}
