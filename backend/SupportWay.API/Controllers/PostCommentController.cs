using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SupportWay.API.Services.Interface;

namespace SupportWay.API.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class PostCommentsController : ControllerBase
    {
        private readonly IPostCommentService _commentService;

        public PostCommentsController(IPostCommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetByPost(Guid postId)
        {
            var comments = await _commentService.GetCommentsByPostAsync(postId);
            return Ok(comments);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreatePostCommentDto dto)
        {
            var userId = User.FindFirst("sub")?.Value ?? User.Identity.Name;
            await _commentService.AddCommentAsync(dto, userId);
            return Ok();
        }

        [Authorize]
        [HttpDelete("{commentId}")]
        public async Task<IActionResult> Delete(Guid commentId)
        {
            var userId = User.FindFirst("sub")?.Value ?? User.Identity.Name;
            await _commentService.DeleteCommentAsync(commentId, userId);
            return Ok();
        }
    }

}
