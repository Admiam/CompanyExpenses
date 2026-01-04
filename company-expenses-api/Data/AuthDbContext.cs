using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CompanyExpenses.Api.Data;

/// <summary>
/// DbContext for accessing the auth database (specifically for roles)
/// </summary>
public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<IdentityRole> Roles { get; set; }
}
