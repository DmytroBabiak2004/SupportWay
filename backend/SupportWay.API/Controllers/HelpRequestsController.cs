using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportWay.Data.DTOs;
using SupportWay.API.DTOs;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HelpRequestsController : ControllerBase
{
    private readonly IHelpRequestService _service;

    public HelpRequestsController(IHelpRequestService service)
        => _service = service;

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

    /// <summary>GET /api/HelpRequests/{id} — повертає HelpRequestDto (feed-сумісний)</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _service.GetHelpRequestByIdAsync(id, userId);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// GET /api/HelpRequests/{id}/details — повна картка для side panel карти.
    /// AllowAnonymous, бо карта публічна.
    /// </summary>
    [HttpGet("{id}/details")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDetails(Guid id)
    {
        var result = await _service.GetHelpRequestDetailsAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] CreateHelpRequestRequest request,
        [FromQuery] double? latitude,
        [FromQuery] double? longitude)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var dto = new HelpRequestCreateDto
        {
            Title = request.Title,
            Content = request.Content,
            LocationId = request.LocationId,
            Latitude = latitude ?? request.Latitude,
            Longitude = longitude ?? request.Longitude,
            Address = request.Address,
            DistrictName = request.DistrictName,
            PreferredDonationMethod = request.PreferredDonationMethod,
            DonationRecipientName = request.DonationRecipientName,
            DonationRecipientCardNumber = request.DonationRecipientCardNumber,
            DonationRecipientIban = request.DonationRecipientIban,
            DonationPaymentLink = request.DonationPaymentLink,
            DonationNotes = request.DonationNotes
        };

        if (request.Image != null)
        {
            using var ms = new MemoryStream();
            await request.Image.CopyToAsync(ms);
            dto.Image = ms.ToArray();
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