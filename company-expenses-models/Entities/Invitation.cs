using CompanyExpenses.Models.Enums;

namespace CompanyExpenses.Models.Entities;

public class Invitation : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string? InvitedRoleId { get; set; } // AspNetRoles.Id
    public Guid? WorkplaceId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public string InvitedByUserId { get; set; } = string.Empty; // AspNetUsers.Id
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

    // Navigation properties
    public Workplace? Workplace { get; set; }
}
