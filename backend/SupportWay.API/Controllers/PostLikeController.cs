using Microsoft.AspNetCore.Mvc;
using SupportWay.API.DTOs;
using SupportWay.Core.Services;

namespace SupportWay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostLikesController : ControllerBase
    {
        private readonly PostLikeService _postLikeService;

        public PostLikesController(PostLikeService postLikeService)
        {
            _postLikeService = postLikeService;
        }

        [HttpPost("like")]
        public async Task<IActionResult> LikePost([FromBody] PostLikeDto dto)
        {
            await _postLikeService.AddLikeAsync(dto);
            return Ok(new { message = "Liked" });
        }

        [HttpPost("unlike")]
        public async Task<IActionResult> UnlikePost([FromBody] PostLikeDto dto)
        {
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
