using Microsoft.AspNetCore.Mvc;
using SupportWay.API.DTOs;
using SupportWay.API.Services.Interface;
using System.Security.Claims;

namespace SupportWay.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FaqBotController : ControllerBase
{
    private readonly IFaqBotService _faqBotService;

    public FaqBotController(IFaqBotService faqBotService)
    {
        _faqBotService = faqBotService;
    }

    [HttpPost("ask")]
    public async Task<ActionResult<FaqBotResponseDto>> Ask([FromBody] FaqBotRequestDto request, CancellationToken cancellationToken)
    {
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
        var response = await _faqBotService.AskAsync(request.Question, roles, cancellationToken);
        return Ok(response);
    }

    [HttpGet("suggestions")]
    public async Task<ActionResult<IReadOnlyList<FaqSuggestionDto>>> GetSuggestions(CancellationToken cancellationToken)
    {
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
        var suggestions = await _faqBotService.GetSuggestionsAsync(roles, cancellationToken);
        return Ok(suggestions);
    }
}
