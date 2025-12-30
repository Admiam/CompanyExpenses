namespace CompanyExpenses.Models.Entities;

public class WorkplaceLimit : BaseEntity
{
    public Guid WorkplaceId { get; set; }
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    public decimal LimitAmount { get; set; }
    public string Currency { get; set; } = "CZK"; // "CZK", "EUR"...

    // Navigation properties
    public Workplace? Workplace { get; set; }
}
