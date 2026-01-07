using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;
using CompanyExpenses.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CompanyExpenses.Api.Controllers;

/// <summary>
/// Controller for workplace management operations including CRUD and dependency checking.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WorkplacesController : ControllerBase
{
    private readonly IWorkplaceService _workplaceService;
    private readonly ILogger<WorkplacesController> _logger;

    public WorkplacesController(
        IWorkplaceService workplaceService,
        ILogger<WorkplacesController> logger)
    {
        _workplaceService = workplaceService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all workplaces in the system.
    /// </summary>
    /// <returns>A list of all workplaces with their members.</returns>
    [HttpGet]
    public async Task<ActionResult> GetWorkplaces()
    {
        _logger.LogInformation("Fetching all workplaces");
        var result = await _workplaceService.GetAllWorkplacesAsync();
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves a single workplace by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the workplace.</param>
    /// <returns>The workplace details if found, otherwise NotFound.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetWorkplace(Guid id)
    {
        _logger.LogInformation("Fetching workplace with ID: {WorkplaceId}", id);
        var result = await _workplaceService.GetWorkplaceByIdAsync(id);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Workplace not found with ID: {WorkplaceId}", id);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Creates a new workplace in the system.
    /// </summary>
    /// <param name="dto">The workplace creation data transfer object.</param>
    /// <returns>The created workplace with its ID, or an error response.</returns>
    [HttpPost]
    public async Task<ActionResult> CreateWorkplace([FromBody] CreateWorkplaceDto dto)
    {
        var userId = GetCurrentUserId() ?? "system";
        _logger.LogInformation("Creating workplace '{WorkplaceName}' by user {UserId}", dto.Name, userId);

        var result = await _workplaceService.CreateWorkplaceAsync(dto, userId);

        if (result.IsSuccess && result.Data != null)
        {
            _logger.LogInformation("Workplace created successfully with ID: {WorkplaceId}", result.Data.Id);
            return CreatedAtAction(nameof(GetWorkplace), new { id = result.Data.Id }, result.Data);
        }

        _logger.LogError("Failed to create workplace: {ErrorMessage}", result.ErrorMessage);
        return HandleResult(result);
    }

    /// <summary>
    /// Updates an existing workplace's information.
    /// </summary>
    /// <param name="id">The unique identifier of the workplace to update.</param>
    /// <param name="dto">The workplace update data transfer object.</param>
    /// <returns>NoContent on success, or error response.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWorkplace(Guid id, [FromBody] UpdateWorkplaceDto dto)
    {
        _logger.LogInformation("Updating workplace {WorkplaceId}", id);
        var result = await _workplaceService.UpdateWorkplaceAsync(id, dto);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Workplace {WorkplaceId} updated successfully", id);
            return NoContent();
        }

        _logger.LogWarning("Failed to update workplace {WorkplaceId}: {ErrorMessage}", id, result.ErrorMessage);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves dependency information for a workplace (members, limits, invitations, expenses).
    /// </summary>
    /// <param name="id">The unique identifier of the workplace.</param>
    /// <returns>Dependency counts and whether the workplace can be deleted.</returns>
    [HttpGet("{id}/dependencies")]
    public async Task<ActionResult> GetWorkplaceDependencies(Guid id)
    {
        _logger.LogInformation("Fetching dependencies for workplace {WorkplaceId}", id);
        var result = await _workplaceService.GetDependenciesAsync(id);
        return HandleResult(result);
    }

    /// <summary>
    /// Deletes a workplace. Only possible if there are no dependencies.
    /// </summary>
    /// <param name="id">The unique identifier of the workplace to delete.</param>
    /// <returns>NoContent on success, or error response if dependencies exist.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkplace(Guid id)
    {
        _logger.LogInformation("Attempting to delete workplace {WorkplaceId}", id);
        var result = await _workplaceService.DeleteWorkplaceAsync(id);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Workplace {WorkplaceId} deleted successfully", id);
            return NoContent();
        }

        _logger.LogWarning("Failed to delete workplace {WorkplaceId}: {ErrorMessage}", id, result.ErrorMessage);
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
