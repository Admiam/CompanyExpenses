using Microsoft.AspNetCore.Mvc;

namespace CompanyExpenses.Api.Controllers;

/// <summary>
/// Controller for health check endpoints used for monitoring and load balancer health probes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Returns the health status of the API service.
    /// </summary>
    /// <returns>Health status with current timestamp.</returns>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
