using Microsoft.AspNetCore.Mvc;
using SupportWay.API.DTOs;
using SupportWay.API.Services.Interfaces;

namespace SupportWay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BadgesController : ControllerBase
    {
        private readonly IBadgeService _badgeService;

        public BadgesController(IBadgeService badgeService)
        {
            _badgeService = badgeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var badges = await _badgeService.GetAllAsync();
            return Ok(badges);
        }

        [HttpGet("profile/{profileId:guid}")]
        public async Task<IActionResult> GetByProfileId(Guid profileId)
        {
            var badges = await _badgeService.GetByProfileIdAsync(profileId);
            return Ok(badges);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var badge = await _badgeService.GetByIdAsync(id);

            if (badge == null)
                return NotFound("Нагороду не знайдено.");

            return Ok(badge);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateBadgeRequest request)
        {
            var badgeId = await _badgeService.CreateBadgeAsync(request);
            return Ok(new { Id = badgeId });
        }

        [HttpPut]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update([FromForm] UpdateBadgeRequest request)
        {
            await _badgeService.UpdateBadgeAsync(request);
            return Ok();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _badgeService.DeleteBadgeAsync(id);
            return Ok();
        }
    }
}