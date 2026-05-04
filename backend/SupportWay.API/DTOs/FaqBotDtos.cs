namespace SupportWay.API.DTOs;

public class FaqBotRequestDto
{
    public string Question { get; set; } = string.Empty;
}

public class FaqQuickActionDto
{
    public string Label { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
}

public class FaqBotResponseDto
{
    public string Answer { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<FaqQuickActionDto> Actions { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
}

public class FaqSuggestionDto
{
    public string Text { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}
