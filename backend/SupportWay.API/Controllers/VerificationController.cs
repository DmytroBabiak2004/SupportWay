using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportWay.API.DTOs;
using SupportWay.API.Services.Interface;
using SupportWay.Data.Models;
using System.Security.Claims;

namespace SupportWay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VerificationController : ControllerBase
    {
        private readonly IVerificationService _service;

        public VerificationController(IVerificationService service)
        {
            _service = service;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet("my")]
        public async Task<IActionResult> GetMyRequest()
        {
            var req = await _service.GetMyPendingRequestAsync(CurrentUserId);
            return Ok(req);
        }

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] SubmitVerificationDto dto)
        {
            try
            {
                var id = await _service.SubmitRequestAsync(CurrentUserId, dto);
                return StatusCode(201, new { id });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ── Admin endpoints ────────────────────────────────────────────────

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] VerificationStatus? status = null)
        {
            var list = await _service.GetAllAsync(status);
            return Ok(list);
        }

        [HttpPost("admin/{id}/decide")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Decide(Guid id, [FromBody] DecideVerificationDto dto)
        {
            try
            {
                await _service.DecideAsync(id, dto, CurrentUserId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
