public class HelpRequestCreateDto
{
    public string Content { get; set; } = string.Empty;
    public int LocationId { get; set; }
    public int RequestStatusId { get; set; }
}

public class HelpRequestDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public decimal TotalPayments { get; set; }
}
