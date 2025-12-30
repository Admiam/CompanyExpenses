using CompanyExpenses.Models.Enums;

namespace CompanyExpenses.Models.Entities;

public class Expense : AuditableEntity
{
    public string EmployeeUserId { get; set; } = string.Empty; // AspNetUsers.Id
    public Guid WorkplaceId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "CZK";
    public DateOnly ExpenseDate { get; set; }
    public string? Description { get; set; }
    public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastDecisionAt { get; set; }
    public string? LastDecisionBy { get; set; } 
    public string? RejectionNote { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public Workplace? Workplace { get; set; }
    public ExpenseCategory? Category { get; set; }
    public ICollection<ExpenseAttachment> Attachments { get; set; } = new List<ExpenseAttachment>();
    public ICollection<ExpenseApproval> Approvals { get; set; } = new List<ExpenseApproval>();
}
