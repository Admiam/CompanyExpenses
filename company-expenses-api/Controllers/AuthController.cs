using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CompanyExpenses.Api.Controllers;

/// <summary>
/// Controller for authentication-related operations including user verification and logout.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Verifies authentication status and returns current user information.
    /// </summary>
    /// <returns>User information if authenticated, otherwise Unauthorized.</returns>
    [HttpGet("user")]
    public IActionResult GetUser()
    {
        _logger.LogInformation("Authentication check initiated");
        _logger.LogDebug("IsAuthenticated: {IsAuth}, Cookie present: {HasCookie}",
            User.Identity?.IsAuthenticated,
            Request.Cookies.ContainsKey(".AspNetCore.Identity.Application"));

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var name = User.FindFirstValue(ClaimTypes.Name);
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            _logger.LogInformation("User authenticated successfully: {Email}", email);

            return Ok(new
            {
                id = userId,
                email = email ?? "",
                name = name ?? "",
                role = roles.FirstOrDefault() ?? "employee"
            });
        }

        _logger.LogWarning("Authentication check failed - user not authenticated");
        return Unauthorized(new { error = "Not authenticated" });
    }

    /// <summary>
    /// Simple authentication status check endpoint.
    /// </summary>
    /// <returns>Object indicating authentication status.</returns>
    [HttpGet("check")]
    public IActionResult CheckAuth()
    {
        var isAuthenticated = User.Identity?.IsAuthenticated == true;
        _logger.LogDebug("Auth check result: {IsAuthenticated}", isAuthenticated);

        return Ok(new { isAuthenticated });
    }

    /// <summary>
    /// Protected endpoint that returns detailed information about the current authenticated user.
    /// </summary>
    /// <returns>Current user's ID, email, name, and roles.</returns>
    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var name = User.FindFirstValue(ClaimTypes.Name);
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        _logger.LogInformation("Current user info requested for: {Email}", email);

        return Ok(new
        {
            id = userId,
            email = email,
            name = name,
            roles = roles
        });
    }

    /// <summary>
    /// Logs out the current user by clearing the authentication cookie.
    /// </summary>
    /// <returns>Success message confirming logout.</returns>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("User logout initiated: {UserId}", userId);

        Response.Cookies.Delete(".AspNetCore.Identity.Application", new CookieOptions
        {
            Path = "/",
            Domain = "localhost",
            SameSite = SameSiteMode.None,
            Secure = true
        });

        _logger.LogInformation("User logged out successfully: {UserId}", userId);
        return Ok(new { message = "Logged out successfully" });
    }
}
