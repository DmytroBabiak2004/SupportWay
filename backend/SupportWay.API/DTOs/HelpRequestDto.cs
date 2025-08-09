public class HelpRequestCreateDto
{
    public string Content { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public Guid RequestStatusId { get; set; }
}

public class HelpRequestDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public decimal TotalPayments { get; set; }
}
