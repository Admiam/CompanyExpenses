using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;
using CompanyExpenses.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CompanyExpenses.Api.Controllers;

/// <summary>
/// Controller for expense category management operations including CRUD and activation/deactivation.
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
    /// Retrieves all expense categories in the system.
    /// </summary>
    /// <returns>A list of all categories including inactive ones.</returns>
    [HttpGet]
    public async Task<ActionResult> GetCategories()
    {
        _logger.LogInformation("Fetching all expense categories");
        var result = await _categoryService.GetAllCategoriesAsync();
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves only active expense categories.
    /// </summary>
    /// <returns>A list of active categories.</returns>
    [HttpGet("active")]
    public async Task<ActionResult> GetActiveCategories()
    {
        _logger.LogInformation("Fetching active expense categories");
        var result = await _categoryService.GetActiveCategoriesAsync();
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves a single expense category by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the category.</param>
    /// <returns>The category details if found, otherwise NotFound.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetCategory(Guid id)
    {
        _logger.LogInformation("Fetching category with ID: {CategoryId}", id);
        var result = await _categoryService.GetCategoryByIdAsync(id);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Category not found with ID: {CategoryId}", id);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Creates a new expense category.
    /// </summary>
    /// <param name="dto">The category creation data transfer object.</param>
    /// <returns>The created category with its ID, or an error response.</returns>
    [HttpPost]
    public async Task<ActionResult> CreateCategory([FromBody] CreateExpenseCategoryDto dto)
    {
        var userId = GetCurrentUserId() ?? "system";
        _logger.LogInformation("Creating category '{CategoryName}' by user {UserId}", dto.Name, userId);

        var result = await _categoryService.CreateCategoryAsync(dto, userId);

        if (result.IsSuccess && result.Data != null)
        {
            _logger.LogInformation("Category created successfully with ID: {CategoryId}", result.Data.Id);
            return CreatedAtAction(nameof(GetCategory), new { id = result.Data.Id }, result.Data);
        }

        _logger.LogWarning("Failed to create category: {ErrorMessage}", result.ErrorMessage);
        return HandleResult(result);
    }

    /// <summary>
    /// Updates an existing expense category.
    /// </summary>
    /// <param name="id">The unique identifier of the category to update.</param>
    /// <param name="dto">The category update data transfer object.</param>
    /// <returns>NoContent on success, or error response.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateExpenseCategoryDto dto)
    {
        _logger.LogInformation("Updating category {CategoryId}", id);
        var result = await _categoryService.UpdateCategoryAsync(id, dto);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Category {CategoryId} updated successfully", id);
            return NoContent();
        }

        _logger.LogWarning("Failed to update category {CategoryId}: {ErrorMessage}", id, result.ErrorMessage);
        return HandleResult(result);
    }

    /// <summary>
    /// Permanently deletes an expense category.
    /// </summary>
    /// <param name="id">The unique identifier of the category to delete.</param>
    /// <returns>NoContent on success, or error response.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        _logger.LogInformation("Deleting category {CategoryId}", id);
        var result = await _categoryService.DeleteCategoryAsync(id);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Category {CategoryId} deleted successfully", id);
            return NoContent();
        }

        _logger.LogWarning("Failed to delete category {CategoryId}: {ErrorMessage}", id, result.ErrorMessage);
        return HandleResult(result);
    }

    /// <summary>
    /// Activates a previously deactivated expense category.
    /// </summary>
    /// <param name="id">The unique identifier of the category to activate.</param>
    /// <returns>Success message or error response.</returns>
    [HttpPatch("activate/{id}")]
    public async Task<IActionResult> ActivateCategory(Guid id)
    {
        _logger.LogInformation("Activating category {CategoryId}", id);
        var result = await _categoryService.ActivateCategoryAsync(id);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Category {CategoryId} activated successfully", id);
            return Ok(new { message = "Category activated successfully" });
        }

        _logger.LogWarning("Failed to activate category {CategoryId}: {ErrorMessage}", id, result.ErrorMessage);
        return HandleResult(result);
    }

    /// <summary>
    /// Deactivates an expense category, making it unavailable for new expenses.
    /// </summary>
    /// <param name="id">The unique identifier of the category to deactivate.</param>
    /// <returns>Success message or error response.</returns>
    [HttpPatch("deactivate/{id}")]
    public async Task<IActionResult> DeactivateCategory(Guid id)
    {
        _logger.LogInformation("Deactivating category {CategoryId}", id);
        var result = await _categoryService.DeactivateCategoryAsync(id);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Category {CategoryId} deactivated successfully", id);
            return Ok(new { message = "Category deactivated successfully" });
        }

        _logger.LogWarning("Failed to deactivate category {CategoryId}: {ErrorMessage}", id, result.ErrorMessage);
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
