using Microsoft.AspNetCore.Identity;

namespace CompanyExpenses.Api.Data;

/// <summary>
/// Custom application user extending IdentityUser with additional properties
/// This mirrors the ApplicationUser in the auth server
/// </summary>
public class ApplicationUser : IdentityUser
{
    public bool IsActive { get; set; } = true;
}
