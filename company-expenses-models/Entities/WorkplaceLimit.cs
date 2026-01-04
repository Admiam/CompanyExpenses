namespace CompanyExpenses.Models.Entities;

public class WorkplaceLimit : AuditableEntity
{
    public Guid WorkplaceId { get; set; }
    public Guid? CategoryId { get; set; }
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    public decimal LimitAmount { get; set; }
    public string Currency { get; set; } = "CZK"; // "CZK", "EUR"...
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Workplace? Workplace { get; set; }
    public ExpenseCategory? Category { get; set; }
}
