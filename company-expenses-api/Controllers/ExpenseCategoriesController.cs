using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;
using CompanyExpenses.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CompanyExpenses.Api.Controllers;

/// <summary>
/// Controller for expense category management - refactored to use Service layer
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ExpenseCategoriesController : ControllerBase
{
    private readonly IExpenseCategoryService _categoryService;
    private readonly ILogger<ExpenseCategoriesController> _logger;

    public ExpenseCategoriesController(
        IExpenseCategoryService categoryService,
        ILogger<ExpenseCategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetCategories()
    {
        var result = await _categoryService.GetAllCategoriesAsync();
        return HandleResult(result);
    }

    /// <summary>
    /// Get active categories
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult> GetActiveCategories()
    {
        var result = await _categoryService.GetActiveCategoriesAsync();
        return HandleResult(result);
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetCategory(Guid id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> CreateCategory([FromBody] CreateExpenseCategoryDto dto)
    {
        var userId = GetCurrentUserId() ?? "system";
        var result = await _categoryService.CreateCategoryAsync(dto, userId);

        if (result.IsSuccess && result.Data != null)
        {
            return CreatedAtAction(nameof(GetCategory), new { id = result.Data.Id }, result.Data);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Update existing category
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateExpenseCategoryDto dto)
    {
        var result = await _categoryService.UpdateCategoryAsync(id, dto);
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return HandleResult(result);
    }

    /// <summary>
    /// Delete category
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id);
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return HandleResult(result);
    }

    /// <summary>
    /// Activate category
    /// </summary>
    [HttpPatch("activate/{id}")]
    public async Task<IActionResult> ActivateCategory(Guid id)
    {
        var result = await _categoryService.ActivateCategoryAsync(id);
        if (result.IsSuccess)
        {
            return Ok(new { message = "Category activated successfully" });
        }
        return HandleResult(result);
    }

    /// <summary>
    /// Deactivate category
    /// </summary>
    [HttpPatch("deactivate/{id}")]
    public async Task<IActionResult> DeactivateCategory(Guid id)
    {
        var result = await _categoryService.DeactivateCategoryAsync(id);
        if (result.IsSuccess)
        {
            return Ok(new { message = "Category deactivated successfully" });
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
