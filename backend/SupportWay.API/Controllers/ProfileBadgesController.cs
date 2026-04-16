using Microsoft.AspNetCore.Mvc;
using SupportWay.API.DTOs;
using SupportWay.API.Services.Interfaces;

namespace SupportWay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileBadgesController : ControllerBase
    {
        private readonly IProfileBadgeService _profileBadgeService;

        public ProfileBadgesController(IProfileBadgeService profileBadgeService)
        {
            _profileBadgeService = profileBadgeService;
        }

        /// <summary>
        /// Видаляє нагороду з профілю (не видаляє саму нагороду з системи).
        /// </summary>
        [HttpDelete("profile/{profileId:guid}/badge/{badgeId:guid}")]
        public async Task<IActionResult> RemoveBadgeFromProfile(Guid profileId, Guid badgeId)
        {
            try
            {
                await _profileBadgeService.RemoveBadgeFromProfileAsync(profileId, badgeId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Видає нагороду профілю (бонус). Перевіряє існування Badge разом із його BadgeType.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AwardBadgeToProfile([FromBody] AwardBadgeRequest request)
        {
            try
            {
                await _profileBadgeService.AwardBadgeToProfileAsync(request.ProfileId, request.BadgeId);
                return Ok(new { message = "Нагороду успішно видано профілю." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}