namespace CompanyExpenses.Models.Entities;

/// <summary>
/// Base entity with common properties for all entities
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty; // AspNetUsers.Id
}

/// <summary>
/// Base entity with audit fields (CreatedAt, CreatedBy, UpdatedAt)
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTime? UpdatedAt { get; set; }
}
