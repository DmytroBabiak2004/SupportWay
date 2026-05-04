using SupportWay.API.DTOs;

namespace SupportWay.API.Services.Interface;

public interface IFaqBotService
{
    Task<FaqBotResponseDto> AskAsync(string question, IEnumerable<string> userRoles, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FaqSuggestionDto>> GetSuggestionsAsync(IEnumerable<string> userRoles, CancellationToken cancellationToken = default);
}
