using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportWay.API.DTOs;
using SupportWay.Services.Interfaces;
using System.Security.Claims;

namespace SupportWay.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/chat")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpGet("{chatId:guid}/messages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetChatHistory(Guid chatId)
        {
            var history = await _messageService.GetHistoryAsync(chatId);
            return Ok(history);
        }

        [HttpPut("messages/{messageId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<MessageDto>> EditMessage(Guid messageId, [FromBody] UpdateMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest("Повідомлення не може бути порожнім");

            var userId = GetCurrentUserId();
            var updated = await _messageService.UpdateAsync(messageId, userId, request.Content);

            if (updated == null)
                return Forbid(); 

            return Ok(updated);
        }


        [HttpDelete("messages/{messageId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteMessage(Guid messageId)
        {
            var userId = GetCurrentUserId();
            var success = await _messageService.DeleteAsync(messageId, userId);

            if (!success)
                return Forbid();

            return NoContent();
        }

        [HttpPost("messages/{messageId:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid messageId)
        {
            var success = await _messageService.MarkAsReadAsync(messageId);
            return success ? Ok() : NotFound();
        }

        private string GetCurrentUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User ID not found in claims");
    }
}