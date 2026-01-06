using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;
using CompanyExpenses.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ApiDTOs = CompanyExpenses.Api.DTOs;

namespace CompanyExpenses.Api.Controllers;

/// <summary>
/// Controller for expense management - refactored to use Service layer
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
    /// Get filtered list of expenses
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetExpenses(
        [FromQuery] Guid? workplaceId = null,
        [FromQuery] string? employeeUserId = null,
        [FromQuery] string? status = null)
    {
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
    /// Get expense by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetExpense(Guid id)
    {
        var result = await _expenseService.GetExpenseByIdAsync(id);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new expense with attachments
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> CreateExpense([FromBody] ApiDTOs.CreateExpenseDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        // Map to service DTO
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
            return CreatedAtAction(nameof(GetExpense), new { id = result.Data.Id }, result.Data);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Update expense amount (only for Pending expenses)
    /// </summary>
    [HttpPatch("{id}/amount")]
    public async Task<IActionResult> UpdateExpenseAmount(Guid id, [FromBody] ApiDTOs.UpdateAmountRequest request)
    {
        var result = await _expenseService.UpdateExpenseAmountAsync(id, request.Amount);
        return HandleResult(result, "Částka byla úspěšně aktualizována");
    }

    /// <summary>
    /// Update expense category (only for Pending expenses)
    /// </summary>
    [HttpPatch("{id}/category")]
    public async Task<IActionResult> UpdateExpenseCategory(Guid id, [FromBody] ApiDTOs.UpdateCategoryRequest request)
    {
        var result = await _expenseService.UpdateExpenseCategoryAsync(id, request.CategoryId);
        return HandleResult(result, "Kategorie byla úspěšně aktualizována");
    }

    /// <summary>
    /// Update expense attachments (only for Pending expenses)
    /// </summary>
    [HttpPatch("{id}/attachments")]
    public async Task<IActionResult> UpdateExpenseAttachments(Guid id, [FromBody] ApiDTOs.UpdateAttachmentsRequest request)
    {
        var attachments = request.Attachments?.Select(a => new AttachmentUploadDto
        {
            FileName = a.FileName,
            FileType = a.FileType,
            Base64Data = a.Base64Data
        }).ToList();

        var result = await _expenseService.UpdateExpenseAttachmentsAsync(id, attachments);
        return HandleResult(result, "Přílohy byly úspěšně aktualizovány");
    }

    /// <summary>
    /// Approve expense
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveExpense(Guid id, [FromBody] ApiDTOs.ApprovalRequest? request = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var result = await _expenseService.ApproveExpenseAsync(id, userId, request?.Note);
        return HandleResult(result, "Expense approved successfully");
    }

    /// <summary>
    /// Reject expense
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectExpense(Guid id, [FromBody] ApiDTOs.ApprovalRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        if (string.IsNullOrWhiteSpace(request?.Note))
        {
            return BadRequest(new { message = "Rejection note is required" });
        }

        var result = await _expenseService.RejectExpenseAsync(id, userId, request.Note);
        return HandleResult(result, "Expense rejected successfully");
    }

    /// <summary>
    /// Delete expense (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(Guid id)
    {
        var result = await _expenseService.DeleteExpenseAsync(id);
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return HandleResult(result);
    }

    /// <summary>
    /// Get dashboard statistics
    /// </summary>
    [HttpGet("dashboard-stats")]
    public async Task<ActionResult> GetDashboardStats()
    {
        var result = await _expenseService.GetDashboardStatsAsync();
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
