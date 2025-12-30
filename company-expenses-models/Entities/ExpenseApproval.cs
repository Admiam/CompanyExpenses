using CompanyExpenses.Models.Enums;

namespace CompanyExpenses.Models.Entities;

public class ExpenseApproval : BaseEntity
{
    public Guid ExpenseId { get; set; }
    public ApprovalAction Action { get; set; }
    public string ActorUserId { get; set; } = string.Empty; // AspNetUsers.Id
    public string? Note { get; set; }

    // Navigation properties
    public Expense? Expense { get; set; }
}
