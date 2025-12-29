using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SupportWay.Data.Models;
using SupportWay.Services.Interfaces;
using System.Security.Claims;

namespace SupportWay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet]
        public async Task<IActionResult> GetChatsForCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var chats = await _chatService.GetChatsByUserIdAsync(userId);
            return Ok(chats);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetChatById(Guid id)
        {
            var chat = await _chatService.GetByIdAsync(id);
            if (chat == null) return NotFound();

            return Ok(chat);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatRequest request)
        {
            if (string.IsNullOrEmpty(request.User1Id) || string.IsNullOrEmpty(request.User2Id))
                return BadRequest("User ids are required");

            Console.WriteLine($"USER 1 ID = {request.User1Id}");
            Console.WriteLine($"USER 2 ID = {request.User2Id}");

            var chat = await _chatService.AddChatAsync(request.User1Id, request.User2Id);
            return Ok(chat);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChat(Guid id)
        {
            await _chatService.DeleteChatAsync(id);
            return NoContent();
        }

        [HttpGet("{id}/is-user-in-chat")]
        public async Task<IActionResult> IsUserInChat(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var isInChat = await _chatService.IsUserInChatAsync(id, userId);
            return Ok(isInChat);
        }
    }
}
