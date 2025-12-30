namespace CompanyExpenses.Models.Entities;

public class ExpenseCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; } 
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
