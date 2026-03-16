using Microsoft.AspNetCore.Mvc;
using SupportWay.API.Services.Interfaces;

namespace SupportWay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileAnalyticsController : ControllerBase
    {
        private readonly IProfileAnalyticsService _profileAnalyticsService;

        public ProfileAnalyticsController(IProfileAnalyticsService profileAnalyticsService)
        {
            _profileAnalyticsService = profileAnalyticsService;
        }

        [HttpGet("{profileId:guid}")]
        public async Task<IActionResult> GetDashboard(Guid profileId)
        {
            if (profileId == Guid.Empty)
                return BadRequest("ProfileId є обов'язковим.");

            var result = await _profileAnalyticsService.GetDashboardAsync(profileId);

            return Ok(result);
        }
    }
}