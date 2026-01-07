using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;
using CompanyExpenses.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ApiDTOs = CompanyExpenses.Api.DTOs;

namespace CompanyExpenses.Api.Controllers;

/// <summary>
/// Controller for expense management operations including CRUD, approval, and rejection workflows.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;
    private readonly ILogger<ExpensesController> _logger;

    public ExpensesController(
        IExpenseService expenseService,
        ILogger<ExpensesController> logger)
    {
        _expenseService = expenseService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a filtered list of expenses based on query parameters.
    /// </summary>
    /// <param name="workplaceId">workplace ID to filter by.</param>
    /// <param name="employeeUserId">employee user ID to filter by.</param>
    /// <param name="status">status to filter by (Pending, Approved, Rejected).</param>
    /// <returns>A list of expenses matching the filter criteria.</returns>
    [HttpGet]
    public async Task<ActionResult> GetExpenses(
        [FromQuery] Guid? workplaceId = null,
        [FromQuery] string? employeeUserId = null,
        [FromQuery] string? status = null)
    {
        _logger.LogInformation("Fetching expenses with filters - WorkplaceId: {WorkplaceId}, EmployeeUserId: {EmployeeUserId}, Status: {Status}",
            workplaceId, employeeUserId, status);

        var filter = new ExpenseFilterDto
        {
            WorkplaceId = workplaceId,
            EmployeeUserId = employeeUserId,
            Status = status
        };

        var result = await _expenseService.GetExpensesAsync(filter);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves a single expense by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the expense.</param>
    /// <returns>The expense details if found, otherwise NotFound.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetExpense(Guid id)
    {
        _logger.LogInformation("Fetching expense with ID: {ExpenseId}", id);
        var result = await _expenseService.GetExpenseByIdAsync(id);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Expense not found with ID: {ExpenseId}", id);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Creates a new expense with optional attachments.
    /// </summary>
    /// <param name="dto">The expense creation data transfer object.</param>
    /// <returns>The created expense with its ID, or an error response.</returns>
    [HttpPost]
    public async Task<ActionResult> CreateExpense([FromBody] ApiDTOs.CreateExpenseDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            _logger.LogWarning("Unauthorized attempt to create expense - user not authenticated");
            return Unauthorized(new { message = "User not authenticated" });
        }

        _logger.LogInformation("Creating expense for user {UserId} - Amount: {Amount} {Currency}",
            userId, dto.Amount, dto.Currency);

        var serviceDto = new CreateExpenseDto
        {
            Description = dto.Description,
            Amount = dto.Amount,
            Currency = dto.Currency,
            ExpenseDate = dto.ExpenseDate,
            CategoryId = dto.CategoryId,
            WorkplaceId = dto.WorkplaceId,
            Attachments = dto.Attachments?.Select(a => new AttachmentUploadDto
            {
                FileName = a.FileName,
                FileType = a.FileType,
                Base64Data = a.Base64Data
            }).ToList()
        };

        var result = await _expenseService.CreateExpenseAsync(serviceDto, userId);

        if (result.IsSuccess && result.Data != null)
        {
            _logger.LogInformation("Expense created successfully with ID: {ExpenseId}", result.Data.Id);
            return CreatedAtAction(nameof(GetExpense), new { id = result.Data.Id }, result.Data);
        }

        _logger.LogError("Failed to create expense: {ErrorMessage}", result.ErrorMessage);
        return HandleResult(result);
    }

    /// <summary>
    /// Updates the amount of an existing expense. Only applicable for expenses with Pending status.
    /// </summary>
    /// <param name="id">The unique identifier of the expense.</param>
    /// <param name="request">The request containing the new amount.</param>
    /// <returns>Success message or error response.</returns>
    [HttpPatch("{id}/amount")]
    public async Task<IActionResult> UpdateExpenseAmount(Guid id, [FromBody] ApiDTOs.UpdateAmountRequest request)
    {
        _logger.LogInformation("Updating amount for expense {ExpenseId} to {Amount}", id, request.Amount);
        var result = await _expenseService.UpdateExpenseAmountAsync(id, request.Amount);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Expense {ExpenseId} amount updated successfully", id);
        }
        else
        {
            _logger.LogWarning("Failed to update expense {ExpenseId} amount: {ErrorMessage}", id, result.ErrorMessage);
        }

        return HandleResult(result, "Amount updated successfully");
    }

    /// <summary>
    /// Updates the category of an existing expense. Only applicable for expenses with Pending status.
    /// </summary>
    /// <param name="id">The unique identifier of the expense.</param>
    /// <param name="request">The request containing the new category ID.</param>
    /// <returns>Success message or error response.</returns>
    [HttpPatch("{id}/category")]
    public async Task<IActionResult> UpdateExpenseCategory(Guid id, [FromBody] ApiDTOs.UpdateCategoryRequest request)
    {
        _logger.LogInformation("Updating category for expense {ExpenseId} to {CategoryId}", id, request.CategoryId);
        var result = await _expenseService.UpdateExpenseCategoryAsync(id, request.CategoryId);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Expense {ExpenseId} category updated successfully", id);
        }
        else
        {
            _logger.LogWarning("Failed to update expense {ExpenseId} category: {ErrorMessage}", id, result.ErrorMessage);
        }

        return HandleResult(result, "Category updated successfully");
    }

    /// <summary>
    /// Updates the attachments of an existing expense. Only applicable for expenses with Pending status.
    /// </summary>
    /// <param name="id">The unique identifier of the expense.</param>
    /// <param name="request">The request containing the new attachments.</param>
    /// <returns>Success message or error response.</returns>
    [HttpPatch("{id}/attachments")]
    public async Task<IActionResult> UpdateExpenseAttachments(Guid id, [FromBody] ApiDTOs.UpdateAttachmentsRequest request)
    {
        _logger.LogInformation("Updating attachments for expense {ExpenseId}, count: {Count}",
            id, request.Attachments?.Count ?? 0);

        var attachments = request.Attachments?.Select(a => new AttachmentUploadDto
        {
            FileName = a.FileName,
            FileType = a.FileType,
            Base64Data = a.Base64Data
        }).ToList();

        var result = await _expenseService.UpdateExpenseAttachmentsAsync(id, attachments);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Expense {ExpenseId} attachments updated successfully", id);
        }
        else
        {
            _logger.LogWarning("Failed to update expense {ExpenseId} attachments: {ErrorMessage}", id, result.ErrorMessage);
        }

        return HandleResult(result, "Attachments updated successfully");
    }

    /// <summary>
    /// Approves an expense, changing its status to Approved.
    /// </summary>
    /// <param name="id">The unique identifier of the expense to approve.</param>
    /// <param name="request">Optional approval request containing a note.</param>
    /// <returns>Success message or error response.</returns>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveExpense(Guid id, [FromBody] ApiDTOs.ApprovalRequest? request = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            _logger.LogWarning("Unauthorized attempt to approve expense {ExpenseId}", id);
            return Unauthorized(new { message = "User not authenticated" });
        }

        _logger.LogInformation("User {UserId} approving expense {ExpenseId}", userId, id);
        var result = await _expenseService.ApproveExpenseAsync(id, userId, request?.Note);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Expense {ExpenseId} approved successfully by user {UserId}", id, userId);
        }
        else
        {
            _logger.LogWarning("Failed to approve expense {ExpenseId}: {ErrorMessage}", id, result.ErrorMessage);
        }

        return HandleResult(result, "Expense approved successfully");
    }

    /// <summary>
    /// Rejects an expense, changing its status to Rejected. A rejection note is required.
    /// </summary>
    /// <param name="id">The unique identifier of the expense to reject.</param>
    /// <param name="request">The rejection request containing a mandatory note.</param>
    /// <returns>Success message or error response.</returns>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectExpense(Guid id, [FromBody] ApiDTOs.ApprovalRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            _logger.LogWarning("Unauthorized attempt to reject expense {ExpenseId}", id);
            return Unauthorized(new { message = "User not authenticated" });
        }

        if (string.IsNullOrWhiteSpace(request?.Note))
        {
            _logger.LogWarning("Rejection attempt for expense {ExpenseId} without required note", id);
            return BadRequest(new { message = "Rejection note is required" });
        }

        _logger.LogInformation("User {UserId} rejecting expense {ExpenseId}", userId, id);
        var result = await _expenseService.RejectExpenseAsync(id, userId, request.Note);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Expense {ExpenseId} rejected successfully by user {UserId}", id, userId);
        }
        else
        {
            _logger.LogWarning("Failed to reject expense {ExpenseId}: {ErrorMessage}", id, result.ErrorMessage);
        }

        return HandleResult(result, "Expense rejected successfully");
    }

    /// <summary>
    /// Soft deletes an expense by marking it as deleted.
    /// </summary>
    /// <param name="id">The unique identifier of the expense to delete.</param>
    /// <returns>NoContent on success, or error response.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(Guid id)
    {
        _logger.LogInformation("Deleting expense {ExpenseId}", id);
        var result = await _expenseService.DeleteExpenseAsync(id);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Expense {ExpenseId} deleted successfully", id);
            return NoContent();
        }

        _logger.LogWarning("Failed to delete expense {ExpenseId}: {ErrorMessage}", id, result.ErrorMessage);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves dashboard statistics including totals, monthly expenses, and category breakdowns.
    /// </summary>
    /// <returns>Dashboard statistics data.</returns>
    [HttpGet("dashboard-stats")]
    public async Task<ActionResult> GetDashboardStats()
    {
        _logger.LogInformation("Fetching dashboard statistics");
        var result = await _expenseService.GetDashboardStatsAsync();
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
    /// Handles service result and returns appropriate HTTP response with optional success message.
    /// </summary>
    /// <param name="result">The service result to handle.</param>
    /// <param name="successMessage">Optional message to include on success.</param>
    /// <returns>Appropriate HTTP response based on result status.</returns>
    private ActionResult HandleResult(ServiceResult result, string? successMessage = null)
    {
        if (result.IsSuccess)
        {
            return Ok(new { message = successMessage ?? "Operation completed successfully" });
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
