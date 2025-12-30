namespace CompanyExpenses.Models.Entities;

public class WorkplaceMember : BaseEntity
{
    public Guid WorkplaceId { get; set; }
    public string UserId { get; set; } = string.Empty; // AspNetUsers.Id
    public string? PositionName { get; set; }
    public bool IsManager { get; set; } = false;

    // Navigation properties
    public Workplace? Workplace { get; set; }
}
