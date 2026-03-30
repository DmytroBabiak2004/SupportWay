using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportWay.Data.DTOs;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HelpRequestsController : ControllerBase
{
    private readonly IHelpRequestService _service;

    public HelpRequestsController(IHelpRequestService service)
    {
        _service = service;
    }

    [HttpGet("feed")]
    public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var list = await _service.GetFeedAsync(userId, page, size);
        return Ok(list);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyHelpRequests([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var list = await _service.GetUserHelpRequestsAsync(userId, page, size);
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetHelpRequestByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] CreateHelpRequestRequest request,
        [FromQuery] double? latitude,   // ← додати
        [FromQuery] double? longitude)  // ← додати
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var dto = new HelpRequestCreateDto
        {
            Title = request.Title,
            Content = request.Content,
            LocationId = request.LocationId,
            // Беремо з query якщо є, інакше з form (може бути null)
            Latitude = latitude ?? request.Latitude,
            Longitude = longitude ?? request.Longitude,
            Address = request.Address,
            DistrictName = request.DistrictName
        };

        if (request.Image != null)
        {
            using var memoryStream = new MemoryStream();
            await request.Image.CopyToAsync(memoryStream);
            dto.Image = memoryStream.ToArray();
        }

        var newId = await _service.CreateHelpRequestAsync(dto, userId);
        return StatusCode(201, new { id = newId });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteHelpRequestAsync(id);
        return NoContent();
    }
}