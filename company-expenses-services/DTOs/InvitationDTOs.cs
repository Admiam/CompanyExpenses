using CompanyExpenses.Models.Enums;

namespace CompanyExpenses.Services.DTOs;

// Invitation DTOs
public class InvitationDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? InvitedRoleId { get; set; }
    public Guid? WorkplaceId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public string InvitedByUserId { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public InvitationWorkplaceDto? Workplace { get; set; }
}

public class InvitationWorkplaceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; }
}

public class CreateInvitationDto
{
    public string Email { get; set; } = string.Empty;
    public string? InvitedRoleId { get; set; }
    public string? Token { get; set; }
    public Guid? WorkplaceId { get; set; }
}
