using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportWay.API.DTOs;
using SupportWay.API.Services.Interface;
using System.Security.Claims;

namespace SupportWay.Api.Controllers
{
    // 1. Створюємо DTO для фото прямо тут або в папці DTOs
    public class ProfilePhotoUpdateDto
    {
        public IFormFile Photo { get; set; }
    }

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

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpPost]
        public async Task<IActionResult> CreateProfile()
        {
            var userId = CurrentUserId; // Використовуємо уніфікований ID
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var existingProfile = await _profileService.GetProfileAsync(userId);
            if (existingProfile != null) return BadRequest("Profile already exists.");

            await _profileService.AddProfileAsync(userId);
            return StatusCode(201);
        }

        [HttpGet("profiles/me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var profileDto = await _profileService.GetProfileAsync(CurrentUserId);

            if (profileDto == null)
            {
                await _profileService.AddProfileAsync(CurrentUserId);
                profileDto = await _profileService.GetProfileAsync(CurrentUserId);
            }

            if (profileDto != null) profileDto.IsOwnProfile = true;

            return Ok(profileDto);
        }

        [HttpGet("profiles/{username}")]
        public async Task<IActionResult> GetProfileByUsername(string username)
        {
            var profileDto = await _profileService.GetProfileByUsernameAsync(username);

            if (profileDto == null) return NotFound();
            profileDto.IsOwnProfile = (profileDto.UserId == CurrentUserId);

            return Ok(profileDto);
        }

        [HttpPut("name")]
        public async Task<IActionResult> UpdateName([FromBody] UpdateProfileNameDto dto)
        {
            await _profileService.UpdateNameAsync(CurrentUserId, dto.Name, dto.FullName);
            return NoContent();
        }

        [HttpPut("description")]
        public async Task<IActionResult> UpdateDescription([FromBody] DescriptionUpdateDto dto)
        {
            await _profileService.UpdateDescriptionAsync(CurrentUserId, dto.Description);
            return NoContent();
        }

        // --- ВИПРАВЛЕНИЙ МЕТОД ---
        [HttpPut("photo")]
        [Consumes("multipart/form-data")] // Вказуємо Swagger тип контенту
        public async Task<IActionResult> UpdatePhoto([FromForm] ProfilePhotoUpdateDto dto)
        {
            if (dto.Photo == null || dto.Photo.Length == 0)
                return BadRequest("Photo is missing.");

            using var memoryStream = new MemoryStream();
            await dto.Photo.CopyToAsync(memoryStream);

            await _profileService.UpdatePhotoAsync(CurrentUserId, memoryStream.ToArray());
            return NoContent();
        }
    }

    public record DescriptionUpdateDto(string Description);
}