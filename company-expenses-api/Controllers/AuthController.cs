using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CompanyExpenses.Api.Controllers;

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
    /// Zkontroluje autentizaci a vrátí info o uživateli
    /// </summary>
    [HttpGet("user")]
    public IActionResult GetUser()
    {
        // Debug logging
        _logger.LogInformation("=== AUTH CHECK ===");
        _logger.LogInformation("IsAuthenticated: {IsAuth}", User.Identity?.IsAuthenticated);
        _logger.LogInformation("Cookie: {Cookie}", Request.Cookies[".AspNetCore.Identity.Application"]);
        _logger.LogInformation("Headers: {Headers}", string.Join(", ", Request.Headers.Select(h => $"{h.Key}={h.Value}")));

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var name = User.FindFirstValue(ClaimTypes.Name);
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            _logger.LogInformation("User authenticated: {Email}", email);

            return Ok(new
            {
                id = userId,
                email = email ?? "",
                name = name ?? "",
                role = roles.FirstOrDefault() ?? "employee"
            });
        }

        _logger.LogWarning("User NOT authenticated");
        return Unauthorized(new { error = "Not authenticated" });
    }

    /// <summary>
    /// Jednoduchá kontrola autentizace
    /// </summary>
    [HttpGet("check")]
    public IActionResult CheckAuth()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Ok(new { isAuthenticated = true });
        }

        return Ok(new { isAuthenticated = false });
    }

    /// <summary>
    /// Info o aktuálním uživateli (protected endpoint)
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var name = User.FindFirstValue(ClaimTypes.Name);
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        return Ok(new
        {
            id = userId,
            email = email,
            name = name,
            roles = roles
        });
    }

    /// <summary>
    /// Logout endpoint - vymaže cookie
    /// </summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Smaž cookie na API straně
        Response.Cookies.Delete(".AspNetCore.Identity.Application", new CookieOptions
        {
            Path = "/",
            Domain = "localhost",
            SameSite = SameSiteMode.None,
            Secure = true
        });

        return Ok(new { message = "Logged out successfully" });
    }
}
