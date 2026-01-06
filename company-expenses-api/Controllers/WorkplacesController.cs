using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;
using CompanyExpenses.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CompanyExpenses.Api.Controllers;

/// <summary>
/// Controller for workplace management - refactored to use Service layer
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
    /// Get all workplaces
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetWorkplaces()
    {
        var result = await _workplaceService.GetAllWorkplacesAsync();
        return HandleResult(result);
    }

    /// <summary>
    /// Get workplace by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetWorkplace(Guid id)
    {
        var result = await _workplaceService.GetWorkplaceByIdAsync(id);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new workplace
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> CreateWorkplace([FromBody] CreateWorkplaceDto dto)
    {
        var userId = GetCurrentUserId() ?? "system";
        var result = await _workplaceService.CreateWorkplaceAsync(dto, userId);

        if (result.IsSuccess && result.Data != null)
        {
            return CreatedAtAction(nameof(GetWorkplace), new { id = result.Data.Id }, result.Data);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Update existing workplace
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWorkplace(Guid id, [FromBody] UpdateWorkplaceDto dto)
    {
        var result = await _workplaceService.UpdateWorkplaceAsync(id, dto);
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return HandleResult(result);
    }

    /// <summary>
    /// Get workplace dependencies
    /// </summary>
    [HttpGet("{id}/dependencies")]
    public async Task<ActionResult> GetWorkplaceDependencies(Guid id)
    {
        var result = await _workplaceService.GetDependenciesAsync(id);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete workplace (only if no dependencies)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkplace(Guid id)
    {
        var result = await _workplaceService.DeleteWorkplaceAsync(id);
        if (result.IsSuccess)
        {
            return NoContent();
        }
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
