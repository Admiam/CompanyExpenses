namespace CompanyExpenses.Api.DTOs;

/// <summary>
/// DTO for WorkplaceMember without circular references
/// </summary>
public class WorkplaceMemberDto
{
    public Guid Id { get; set; }
    public Guid WorkplaceId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? PositionName { get; set; }
    public bool IsManager { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// DTO for Workplace list view
/// </summary>
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

/// <summary>
/// DTO for WorkplaceLimit
/// </summary>
public class WorkplaceLimitDto
{
    public Guid Id { get; set; }
    public Guid WorkplaceId { get; set; }
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    public decimal LimitAmount { get; set; }
    public string Currency { get; set; } = "CZK";
    public Guid? CategoryId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// DTO for Workplace detail view with all related data
/// </summary>
public class WorkplaceDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public List<WorkplaceMemberDto> Members { get; set; } = new();
    public List<WorkplaceLimitDto> Limits { get; set; } = new();
}
