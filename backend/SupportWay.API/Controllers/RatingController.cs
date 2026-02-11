using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportWay.Data.Models;
using System.Security.Claims;
using SupportWay.API.DTOs;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RatingController : ControllerBase
{
    private readonly IProfileRatingService _ratingService;

    public RatingController(IProfileRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    [HttpPost]
    public async Task<IActionResult> Rate([FromBody] RateProfileDto dto)
    {
        var raterId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(raterId)) return Unauthorized();

        var newAverage = await _ratingService.RateProfileAsync(raterId, dto.ProfileId, dto.Value);

        return Ok(new { averageRating = newAverage });
    }
}

