namespace CompanyExpenses.Models.Entities;

public class Workplace : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<WorkplaceMember> Members { get; set; } = new List<WorkplaceMember>();
    public ICollection<WorkplaceLimit> Limits { get; set; } = new List<WorkplaceLimit>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();
}
