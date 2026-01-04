using CompanyExpenses.Database.Data;
using CompanyExpenses.Models.Entities;
using CompanyExpenses.Models.Enums;
using CompanyExpenses.Api.DTOs;
using CompanyExpenses.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CompanyExpenses.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ExpensesController> _logger;
    private readonly IImageCompressionService _imageCompressionService;

    public ExpensesController(
        AppDbContext context,
        ILogger<ExpensesController> logger,
        IImageCompressionService imageCompressionService)
    {
        _context = context;
        _logger = logger;
        _imageCompressionService = imageCompressionService;
    }

    /// <summary>
    /// Získá seznam všech výdajů (s filtrováním)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetExpenses(
        [FromQuery] Guid? workplaceId = null,
        [FromQuery] string? employeeUserId = null,
        [FromQuery] ExpenseStatus? status = null)
    {
        var query = _context.Expenses
            .Include(e => e.Category)
            .Include(e => e.Workplace)
            .AsQueryable();

        if (workplaceId.HasValue)
        {
            query = query.Where(e => e.WorkplaceId == workplaceId.Value);
        }

        if (!string.IsNullOrEmpty(employeeUserId))
        {
            query = query.Where(e => e.EmployeeUserId == employeeUserId);
        }

        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }

        var expenses = await query
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();

        // Map to DTO to avoid circular references
        var result = expenses.Select(e => new
        {
            id = e.Id,
            description = e.Description,
            amount = e.Amount,
            currency = e.Currency,
            expenseDate = e.ExpenseDate,
            status = e.Status.ToString(),
            employeeUserId = e.EmployeeUserId,
            workplaceId = e.WorkplaceId,
            categoryId = e.CategoryId,
            workplace = e.Workplace != null ? new { id = e.Workplace.Id, name = e.Workplace.Name } : null,
            category = e.Category != null ? new { id = e.Category.Id, name = e.Category.Name } : null,
            submittedAt = e.SubmittedAt,
            createdAt = e.CreatedAt
        }).ToList();

        return Ok(result);
    }

    /// <summary>
    /// Získá konkrétní výdaj podle ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Expense>> GetExpense(Guid id)
    {
        var expense = await _context.Expenses
            .Include(e => e.Category)
            .Include(e => e.Workplace)
            .Include(e => e.Attachments)
            .Include(e => e.Approvals)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (expense == null)
        {
            return NotFound();
        }

        return expense;
    }

    /// <summary>
    /// Vytvoří nový výdaj s přílohami
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Expense>> CreateExpense([FromBody] CreateExpenseDto dto)
    {
        try
        {
            // Get authenticated user ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            // Create expense entity
            var expense = new Expense
            {
                Id = Guid.NewGuid(),
                Description = dto.Description,
                Amount = dto.Amount,
                Currency = dto.Currency,
                ExpenseDate = dto.ExpenseDate,
                CategoryId = dto.CategoryId,
                WorkplaceId = dto.WorkplaceId,
                EmployeeUserId = userId,
                Status = ExpenseStatus.Pending,
                SubmittedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.Expenses.Add(expense);

            // Process and add attachments
            if (dto.Attachments != null && dto.Attachments.Any())
            {
                foreach (var attachmentDto in dto.Attachments)
                {
                    try
                    {
                        // Compress image to base64
                        var (compressedBase64, compressedSize) = await _imageCompressionService
                            .CompressImageToBase64Async(attachmentDto.Base64Data, attachmentDto.DataType);

                        var attachment = new ExpenseAttachment
                        {
                            Id = Guid.NewGuid(),
                            ExpenseId = expense.Id,
                            OriginalFileName = attachmentDto.OriginalFileName,
                            StoredFileName = $"{Guid.NewGuid()}{Path.GetExtension(attachmentDto.OriginalFileName)}",
                            DataType = "image/jpeg", // Always JPEG after compression
                            FileSize = compressedSize,
                            Base64Data = compressedBase64,
                            UploadedByUserId = userId,
                            UploadedAt = DateTime.UtcNow
                        };

                        _context.ExpenseAttachments.Add(attachment);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to compress attachment: {FileName}", attachmentDto.OriginalFileName);
                        // Continue with other attachments
                    }
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Expense created: {ExpenseId} by user {UserId}", expense.Id, userId);

            // Return simple response to avoid circular reference issues
            return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, new
            {
                id = expense.Id,
                description = expense.Description,
                amount = expense.Amount,
                currency = expense.Currency,
                expenseDate = expense.ExpenseDate,
                status = expense.Status.ToString(),
                attachmentsCount = dto.Attachments?.Count ?? 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create expense");
            return StatusCode(500, new { message = "Failed to create expense" });
        }
    }

    /// <summary>
    /// Schválí výdaj
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveExpense(Guid id, [FromBody] string? note = null)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense == null)
        {
            return NotFound();
        }

        expense.Status = ExpenseStatus.Approved;
        expense.LastDecisionAt = DateTime.UtcNow;
        expense.LastDecisionBy = "test-manager"; // TODO: Získat z authentication
        expense.UpdatedAt = DateTime.UtcNow;

        // Přidat záznam do historie schvalování
        var approval = new ExpenseApproval
        {
            Id = Guid.NewGuid(),
            ExpenseId = id,
            Action = ApprovalAction.Approved,
            ActorUserId = "test-manager",
            Note = note,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-manager"
        };

        _context.ExpenseApprovals.Add(approval);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Zamítne výdaj
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectExpense(Guid id, [FromBody] string rejectionNote)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense == null)
        {
            return NotFound();
        }

        expense.Status = ExpenseStatus.Rejected;
        expense.LastDecisionAt = DateTime.UtcNow;
        expense.LastDecisionBy = "test-manager";
        expense.RejectionNote = rejectionNote;
        expense.UpdatedAt = DateTime.UtcNow;

        var approval = new ExpenseApproval
        {
            Id = Guid.NewGuid(),
            ExpenseId = id,
            Action = ApprovalAction.Rejected,
            ActorUserId = "test-manager",
            Note = rejectionNote,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-manager"
        };

        _context.ExpenseApprovals.Add(approval);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Smaže výdaj (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(Guid id)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense == null)
        {
            return NotFound();
        }

        expense.IsDeleted = true;
        expense.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
