using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;
using CompanyExpenses.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CompanyExpenses.Api.Controllers;

/// <summary>
/// Controller for workplace spending limit management operations including CRUD and usage tracking.
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
    /// Retrieves all spending limits for a specific workplace.
    /// </summary>
    /// <param name="workplaceId">The unique identifier of the workplace.</param>
    /// <returns>A list of limits for the specified workplace.</returns>
    [HttpGet("workplace/{workplaceId}")]
    public async Task<ActionResult> GetWorkplaceLimits(Guid workplaceId)
    {
        _logger.LogInformation("Fetching limits for workplace {WorkplaceId}", workplaceId);
        var result = await _limitService.GetLimitsByWorkplaceAsync(workplaceId);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves a single spending limit by its unique identifier.
    /// </summary>
    /// <param name="workplaceId">The unique identifier of the workplace.</param>
    /// <param name="id">The unique identifier of the limit.</param>
    /// <returns>The limit details if found, otherwise NotFound.</returns>
    [HttpGet("{workplaceId}/{id}")]
    public async Task<ActionResult> GetLimit(Guid workplaceId, Guid id)
    {
        _logger.LogInformation("Fetching limit {LimitId} for workplace {WorkplaceId}", id, workplaceId);
        var result = await _limitService.GetLimitByIdAsync(workplaceId, id);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Limit not found with ID: {LimitId}", id);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Creates a new spending limit for a workplace.
    /// </summary>
    /// <param name="workplaceId">The unique identifier of the workplace.</param>
    /// <param name="dto">The limit creation data transfer object.</param>
    /// <returns>The created limit with its ID, or an error response.</returns>
    [HttpPost("{workplaceId}")]
    public async Task<ActionResult> CreateLimit(Guid workplaceId, [FromBody] CreateWorkplaceLimitDto dto)
    {
        var userId = GetCurrentUserId() ?? "system";
        _logger.LogInformation("Creating limit for workplace {WorkplaceId} by user {UserId} - Amount: {Amount} {Currency}",
            workplaceId, userId, dto.LimitAmount, dto.Currency);

        var result = await _limitService.CreateLimitAsync(workplaceId, dto, userId);

        if (result.IsSuccess && result.Data != null)
        {
            _logger.LogInformation("Limit created successfully with ID: {LimitId}", result.Data.Id);
            return CreatedAtAction(nameof(GetLimit),
                new { workplaceId = workplaceId, id = result.Data.Id },
                result.Data);
        }

        _logger.LogWarning("Failed to create limit: {ErrorMessage}", result.ErrorMessage);
        return HandleResult(result);
    }

    /// <summary>
    /// Updates an existing spending limit.
    /// </summary>
    /// <param name="workplaceId">The unique identifier of the workplace.</param>
    /// <param name="id">The unique identifier of the limit to update.</param>
    /// <param name="dto">The limit update data transfer object.</param>
    /// <returns>NoContent on success, or error response.</returns>
    [HttpPut("{workplaceId}/{id}")]
    public async Task<IActionResult> UpdateLimit(Guid workplaceId, Guid id, [FromBody] UpdateWorkplaceLimitDto dto)
    {
        _logger.LogInformation("Updating limit {LimitId} for workplace {WorkplaceId}", id, workplaceId);
        var result = await _limitService.UpdateLimitAsync(workplaceId, id, dto);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Limit {LimitId} updated successfully", id);
            return NoContent();
        }

        _logger.LogWarning("Failed to update limit {LimitId}: {ErrorMessage}", id, result.ErrorMessage);
        return HandleResult(result);
    }

    /// <summary>
    /// Permanently deletes a spending limit.
    /// </summary>
    /// <param name="workplaceId">The unique identifier of the workplace.</param>
    /// <param name="id">The unique identifier of the limit to delete.</param>
    /// <returns>NoContent on success, or error response.</returns>
    [HttpDelete("{workplaceId}/{id}")]
    public async Task<IActionResult> DeleteLimit(Guid workplaceId, Guid id)
    {
        _logger.LogInformation("Deleting limit {LimitId} from workplace {WorkplaceId}", id, workplaceId);
        var result = await _limitService.DeleteLimitAsync(workplaceId, id);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Limit {LimitId} deleted successfully", id);
            return NoContent();
        }

        _logger.LogWarning("Failed to delete limit {LimitId}: {ErrorMessage}", id, result.ErrorMessage);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves usage statistics for a specific spending limit.
    /// </summary>
    /// <param name="workplaceId">The unique identifier of the workplace.</param>
    /// <param name="id">The unique identifier of the limit.</param>
    /// <returns>Usage statistics including used amount, remaining amount, and exceeded status.</returns>
    [HttpGet("{workplaceId}/{id}/usage")]
    public async Task<ActionResult> GetLimitUsage(Guid workplaceId, Guid id)
    {
        _logger.LogInformation("Fetching usage for limit {LimitId} in workplace {WorkplaceId}", id, workplaceId);
        var result = await _limitService.GetLimitUsageAsync(workplaceId, id);
        return HandleResult(result);
    }

    #region Helper Methods

    /// <summary>
    /// Gets the current authenticated user's ID from the claims.
    /// </summary>
    /// <returns>The user ID if authenticated, otherwise null.</returns>
    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    /// <summary>
    /// Handles service result and returns appropriate HTTP response for generic results.
    /// </summary>
    /// <typeparam name="T">The type of data in the result.</typeparam>
    /// <param name="result">The service result to handle.</param>
    /// <returns>Appropriate HTTP response based on result status.</returns>
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

    /// <summary>
    /// Handles service result and returns appropriate HTTP response.
    /// </summary>
    /// <param name="result">The service result to handle.</param>
    /// <returns>Appropriate HTTP response based on result status.</returns>
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
