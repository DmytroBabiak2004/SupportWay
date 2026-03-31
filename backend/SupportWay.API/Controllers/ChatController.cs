using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportWay.API.DTOs;
using System.Security.Claims;
using SupportWay.Services.Interfaces;

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

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public async Task<IActionResult> GetChatsForCurrentUser()
        {
            var chats = await _chatService.GetChatsByUserIdAsync(CurrentUserId);
            return Ok(chats);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetChatById(Guid id)
        {
            var chat = await _chatService.GetByIdAsync(id);
            if (chat == null) return NotFound();
            return Ok(chat);
        }

        /// <summary>
        /// Creates a private chat or returns the existing one.
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatRequest request)
        {
            if (string.IsNullOrEmpty(request.User1Id) || string.IsNullOrEmpty(request.User2Id))
                return BadRequest("User ids are required");

            var result = await _chatService.GetOrCreatePrivateChatAsync(
                request.User1Id, request.User2Id);
            return Ok(result);
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
            var isInChat = await _chatService.IsUserInChatAsync(id, CurrentUserId);
            return Ok(isInChat);
        }
    }
}