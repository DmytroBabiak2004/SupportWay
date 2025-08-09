using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportWay.API.Services.Interface;
using System.Security.Claims;

namespace SupportWay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateProfile()
        {
            var userId = User?.Identity?.Name;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var existingProfile = await _profileService.GetProfileAsync(userId);
            if (existingProfile != null)
                return BadRequest("Profile already exists.");

            await _profileService.AddProfileAsync(userId);

            return StatusCode(201);
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _profileService.GetProfileAsync(userId);

            if (profile == null)
                return NotFound("Profile not found.");

            return Ok(profile);
        }

        //[HttpPut("description")]
        //public async Task<IActionResult> UpdateDescription([FromBody] string description)
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    await _profileService.UpdateDescriptionAsync(userId, description);
        //    return NoContent();
        //}

        //[HttpPut("photo")]
        //[Consumes("multipart/form-data")]
        //public async Task<IActionResult> UpdatePhoto([FromForm] IFormFile photo)
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    if (photo == null || photo.Length == 0)
        //        return BadRequest("Photo file is missing.");

        //    using var memoryStream = new MemoryStream();
        //    await photo.CopyToAsync(memoryStream);
        //    var photoBytes = memoryStream.ToArray();

        //    await _profileService.UpdatePhotoAsync(userId, photoBytes);
        //    return NoContent();
        //}

    }
}
