using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CompanyExpenses.Api.Data;

namespace CompanyExpenses.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly AuthDbContext _authContext;
    private readonly ILogger<RolesController> _logger;

    public RolesController(AuthDbContext authContext, ILogger<RolesController> logger)
    {
        _authContext = authContext;
        _logger = logger;
    }

    /// <summary>
    /// Gets all roles from auth database
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
    {
        try
        {
            var roles = await _authContext.Roles
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name ?? string.Empty
                })
                .ToListAsync();

            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get roles from auth database");
            return StatusCode(500, new { message = "Failed to get roles" });
        }
    }
}

public class RoleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
