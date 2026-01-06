using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;
using CompanyExpenses.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CompanyExpenses.Api.Controllers;

/// <summary>
/// Controller for workplace limit management - refactored to use Service layer
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WorkplaceLimitsController : ControllerBase
{
    private readonly IWorkplaceLimitService _limitService;
    private readonly ILogger<WorkplaceLimitsController> _logger;

    public WorkplaceLimitsController(
        IWorkplaceLimitService limitService,
        ILogger<WorkplaceLimitsController> logger)
    {
        _limitService = limitService;
        _logger = logger;
    }

    /// <summary>
    /// Get all limits for a workplace
    /// </summary>
    [HttpGet("workplace/{workplaceId}")]
    public async Task<ActionResult> GetWorkplaceLimits(Guid workplaceId)
    {
        var result = await _limitService.GetLimitsByWorkplaceAsync(workplaceId);
        return HandleResult(result);
    }

    /// <summary>
    /// Get limit by ID
    /// </summary>
    [HttpGet("{workplaceId}/{id}")]
    public async Task<ActionResult> GetLimit(Guid workplaceId, Guid id)
    {
        var result = await _limitService.GetLimitByIdAsync(workplaceId, id);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new limit
    /// </summary>
    [HttpPost("{workplaceId}")]
    public async Task<ActionResult> CreateLimit(Guid workplaceId, [FromBody] CreateWorkplaceLimitDto dto)
    {
        var userId = GetCurrentUserId() ?? "system";
        var result = await _limitService.CreateLimitAsync(workplaceId, dto, userId);

        if (result.IsSuccess && result.Data != null)
        {
            return CreatedAtAction(nameof(GetLimit),
                new { workplaceId = workplaceId, id = result.Data.Id },
                result.Data);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Update existing limit
    /// </summary>
    [HttpPut("{workplaceId}/{id}")]
    public async Task<IActionResult> UpdateLimit(Guid workplaceId, Guid id, [FromBody] UpdateWorkplaceLimitDto dto)
    {
        var result = await _limitService.UpdateLimitAsync(workplaceId, id, dto);
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return HandleResult(result);
    }

    /// <summary>
    /// Delete limit
    /// </summary>
    [HttpDelete("{workplaceId}/{id}")]
    public async Task<IActionResult> DeleteLimit(Guid workplaceId, Guid id)
    {
        var result = await _limitService.DeleteLimitAsync(workplaceId, id);
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return HandleResult(result);
    }

    /// <summary>
    /// Get limit usage statistics
    /// </summary>
    [HttpGet("{workplaceId}/{id}/usage")]
    public async Task<ActionResult> GetLimitUsage(Guid workplaceId, Guid id)
    {
        var result = await _limitService.GetLimitUsageAsync(workplaceId, id);
        return HandleResult(result);
    }

    #region Helper Methods

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private ActionResult HandleResult<T>(ServiceResult<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(new { message = result.ErrorMessage }),
            ServiceErrorType.BadRequest => BadRequest(new { message = result.ErrorMessage }),
            ServiceErrorType.Unauthorized => Unauthorized(new { message = result.ErrorMessage }),
            _ => StatusCode(500, new { message = result.ErrorMessage })
        };
    }

    private ActionResult HandleResult(ServiceResult result)
    {
        if (result.IsSuccess)
        {
            return Ok();
        }

        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(new { message = result.ErrorMessage }),
            ServiceErrorType.BadRequest => BadRequest(new { message = result.ErrorMessage }),
            ServiceErrorType.Unauthorized => Unauthorized(new { message = result.ErrorMessage }),
            _ => StatusCode(500, new { message = result.ErrorMessage })
        };
    }

    #endregion
}
