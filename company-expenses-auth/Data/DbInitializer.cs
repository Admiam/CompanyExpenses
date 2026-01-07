using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace company_expenses_auth.Data;

public static class DbInitializer
{
    public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
    {
        string[] roleNames = { "Admin", "Manager", "User" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    public static async Task SeedAdminUser(
        UserManager<ApplicationUser> userManager,
        ILogger? logger = null)
    {
        // Check if admin user already exists
        const string adminEmail = "admin@company-expenses.local";
        const string adminPassword = "Admin123!";

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin != null)
        {
            logger?.LogInformation("Admin user already exists: {Email}", adminEmail);
            return;
        }

        // Create admin user
        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true, // Pre-confirmed for Docker setup
            IsActive = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            // Assign Admin role
            await userManager.AddToRoleAsync(adminUser, "Admin");
            logger?.LogInformation("Created admin user: {Email} with password: {Password}", adminEmail, adminPassword);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger?.LogError("Failed to create admin user: {Errors}", errors);
        }
    }
}
