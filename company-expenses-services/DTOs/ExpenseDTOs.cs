using CompanyExpenses.Models.Enums;

namespace CompanyExpenses.Services.DTOs;

// Expense DTOs
public class ExpenseListDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "CZK";
    public DateOnly ExpenseDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string EmployeeUserId { get; set; } = string.Empty;
    public Guid? WorkplaceId { get; set; }
    public Guid? CategoryId { get; set; }
    public CategoryInfoDto? Category { get; set; }
    public WorkplaceInfoDto? Workplace { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ExpenseDetailDto : ExpenseListDto
{
    public DateTime? LastDecisionAt { get; set; }
    public string? LastDecisionBy { get; set; }
    public string? RejectionNote { get; set; }
    public List<ExpenseAttachmentDto> Attachments { get; set; } = new();
    public List<ExpenseApprovalDto> Approvals { get; set; } = new();
}

public class ExpenseDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "CZK";
    public DateOnly ExpenseDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int AttachmentsCount { get; set; }
}

public class CreateExpenseDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "CZK";
    public DateOnly ExpenseDate { get; set; }
    public Guid CategoryId { get; set; }
    public Guid WorkplaceId { get; set; }
    public List<AttachmentUploadDto>? Attachments { get; set; }
}

public class AttachmentUploadDto
{
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string Base64Data { get; set; } = string.Empty;
}

public class ExpenseAttachmentDto
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Base64Data { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class ExpenseApprovalDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string ActorEmail { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CategoryInfoDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class WorkplaceInfoDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// Dashboard DTOs
public class DashboardStatsDto
{
    public decimal TotalExpenses { get; set; }
    public decimal MonthlyExpenses { get; set; }
    public decimal MonthlyChange { get; set; }
    public int WorkplacesCount { get; set; }
    public int UsersCount { get; set; }
    public int PendingExpensesCount { get; set; }
    public IEnumerable<object> ExpensesByCategory { get; set; } = new List<object>();
    public IEnumerable<object> ExpensesByWorkplace { get; set; } = new List<object>();
    public IEnumerable<object> RecentExpenses { get; set; } = new List<object>();
}

public class ExpenseFilterDto
{
    public Guid? WorkplaceId { get; set; }
    public string? EmployeeUserId { get; set; }
    public string? Status { get; set; }
}
