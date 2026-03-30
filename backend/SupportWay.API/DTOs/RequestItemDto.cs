using System.ComponentModel.DataAnnotations;

public class RequestItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public Guid SupportTypeId { get; set; }
    public string SupportTypeName { get; set; } // Для виводу назви типу допомоги
}

public class CreateRequestItemDto
{
    [Required]
    public Guid HelpRequestId { get; set; }

    [Required]
    public string Name { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    [Required]
    public Guid SupportTypeId { get; set; }
}