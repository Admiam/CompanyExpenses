namespace CompanyExpenses.Services.DTOs;

// Workplace DTOs
public class WorkplaceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public List<WorkplaceMemberDto> Members { get; set; } = new();
}

public class WorkplaceDetailDto : WorkplaceDto
{
    public List<WorkplaceLimitDto> Limits { get; set; } = new();
}

public class CreateWorkplaceDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateWorkplaceDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; }
}

public class WorkplaceDependenciesDto
{
    public Guid WorkplaceId { get; set; }
    public int MembersCount { get; set; }
    public int LimitsCount { get; set; }
    public int InvitationsCount { get; set; }
    public int ExpensesCount { get; set; }
    public bool CanDelete { get; set; }
}

// Workplace Member DTOs
public class WorkplaceMemberDto
{
    public Guid Id { get; set; }
    public Guid WorkplaceId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
    public string? PositionName { get; set; }
    public bool IsManager { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class CreateWorkplaceMemberDto
{
    public string UserId { get; set; } = string.Empty;
    public string? PositionName { get; set; }
    public bool IsManager { get; set; }
}

public class UpdateWorkplaceMemberDto
{
    public string? PositionName { get; set; }
    public bool IsManager { get; set; }
}

// Workplace Limit DTOs
public class WorkplaceLimitDto
{
    public Guid Id { get; set; }
    public Guid WorkplaceId { get; set; }
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    public decimal LimitAmount { get; set; }
    public string Currency { get; set; } = "CZK";
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class CreateWorkplaceLimitDto
{
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    public decimal LimitAmount { get; set; }
    public string Currency { get; set; } = "CZK";
    public Guid? CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateWorkplaceLimitDto
{
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    public decimal LimitAmount { get; set; }
    public string Currency { get; set; } = "CZK";
    public Guid? CategoryId { get; set; }
    public bool IsActive { get; set; }
}

public class LimitUsageDto
{
    public Guid LimitId { get; set; }
    public decimal LimitAmount { get; set; }
    public decimal UsedAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool IsExceeded { get; set; }
}

// User with stats DTO
public class UserWithStatsDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "employee";
    public string Workplace { get; set; } = string.Empty;
    public Guid? WorkplaceId { get; set; }
    public bool IsActive { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ExpenseCount { get; set; }
    public decimal TotalExpenses { get; set; }
}

public class UserDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "employee";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<UserMembershipDto> Memberships { get; set; } = new();
    public List<UserExpenseDto> Expenses { get; set; } = new();
    public UserExpenseStatsDto ExpenseStats { get; set; } = new();
    public List<UserApprovalDto> Approvals { get; set; } = new();
    public UserApprovalStatsDto ApprovalStats { get; set; } = new();
    public List<UserInvitationDto> Invitations { get; set; } = new();
    public UserInvitationStatsDto InvitationStats { get; set; } = new();
}

public class UserExpenseStatsDto
{
    public decimal Total { get; set; }
    public int Count { get; set; }
    public List<ExpenseStatusStatDto> ByStatus { get; set; } = new();
}

public class ExpenseStatusStatDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Total { get; set; }
}

public class UserApprovalStatsDto
{
    public int Count { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
}

public class UserInvitationDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string WorkplaceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class UserInvitationStatsDto
{
    public int Count { get; set; }
    public int Pending { get; set; }
    public int Accepted { get; set; }
    public int Expired { get; set; }
}

public class UserMembershipDto
{
    public Guid Id { get; set; }
    public Guid WorkplaceId { get; set; }
    public string WorkplaceName { get; set; } = string.Empty;
    public string? PositionName { get; set; }
    public bool IsManager { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class UserExpenseDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "CZK";
    public DateOnly ExpenseDate { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string WorkplaceName { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}

public class UserApprovalDto
{
    public Guid Id { get; set; }
    public Guid ExpenseId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
