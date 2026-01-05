using CompanyExpenses.Database.Data;
using CompanyExpenses.Models.Entities;
using CompanyExpenses.Models.Enums;
using CompanyExpenses.Api.DTOs;
using CompanyExpenses.Api.Services;
using CompanyExpenses.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CompanyExpenses.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly AuthDbContext _authContext;
    private readonly ILogger<ExpensesController> _logger;
    private readonly IImageCompressionService _imageCompressionService;

    public ExpensesController(
        AppDbContext context,
        AuthDbContext authContext,
        ILogger<ExpensesController> logger,
        IImageCompressionService imageCompressionService)
    {
        _context = context;
        _authContext = authContext;
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
    public async Task<ActionResult> GetExpense(Guid id)
    {
        var expense = await _context.Expenses
            .Include(e => e.Category)
            .Include(e => e.Workplace)
            .Include(e => e.Approvals)
            .Include(e => e.Attachments)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (expense == null)
        {
            return NotFound();
        }

        // Get user emails from auth database
        var userIds = expense.Approvals.Select(a => a.ActorUserId).Distinct().ToList();
        if (expense.LastDecisionBy != null)
        {
            userIds.Add(expense.LastDecisionBy);
        }

        var users = await _authContext.NetUsers
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Email ?? u.UserName ?? u.Id);

        // Map to DTO with approvals and attachments
        var result = new
        {
            id = expense.Id,
            description = expense.Description,
            amount = expense.Amount,
            currency = expense.Currency,
            expenseDate = expense.ExpenseDate,
            status = expense.Status.ToString(),
            employeeUserId = expense.EmployeeUserId,
            workplaceId = expense.WorkplaceId,
            categoryId = expense.CategoryId,
            workplace = expense.Workplace != null ? new { id = expense.Workplace.Id, name = expense.Workplace.Name } : null,
            category = expense.Category != null ? new { id = expense.Category.Id, name = expense.Category.Name } : null,
            submittedAt = expense.SubmittedAt,
            createdAt = expense.CreatedAt,
            lastDecisionAt = expense.LastDecisionAt,
            lastDecisionBy = expense.LastDecisionBy != null && users.ContainsKey(expense.LastDecisionBy)
                ? users[expense.LastDecisionBy]
                : expense.LastDecisionBy,
            rejectionNote = expense.RejectionNote,
            attachments = expense.Attachments.Select(a => new
            {
                id = a.Id,
                originalFileName = a.OriginalFileName,
                dataType = a.DataType,
                fileSize = a.FileSize,
                base64Data = a.Base64Data,
                uploadedAt = a.UploadedAt
            }).ToList(),
            approvals = expense.Approvals.Select(a => new
            {
                id = a.Id,
                action = a.Action.ToString(),
                actorEmail = users.ContainsKey(a.ActorUserId) ? users[a.ActorUserId] : a.ActorUserId,
                note = a.Note,
                createdAt = a.CreatedAt
            }).OrderByDescending(a => a.createdAt).ToList()
        };

        return Ok(result);
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
                            .CompressImageToBase64Async(attachmentDto.Base64Data, attachmentDto.FileType);

                        var attachment = new ExpenseAttachment
                        {
                            Id = Guid.NewGuid(),
                            ExpenseId = expense.Id,
                            OriginalFileName = attachmentDto.FileName,
                            StoredFileName = $"{Guid.NewGuid()}{Path.GetExtension(attachmentDto.FileName)}",
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
                        _logger.LogError(ex, "Failed to compress attachment: {FileName}", attachmentDto.FileName);
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
    /// Aktualizuje částku výdaje (pouze pro Pending výdaje)
    /// </summary>
    [HttpPatch("{id}/amount")]
    public async Task<IActionResult> UpdateExpenseAmount(Guid id, [FromBody] UpdateAmountRequest request)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense == null)
        {
            return NotFound();
        }

        // Allow updating only for pending expenses
        if (expense.Status != ExpenseStatus.Pending)
        {
            return BadRequest(new { message = "Lze upravit pouze výdaje čekající na schválení" });
        }

        expense.Amount = request.Amount;
        expense.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Částka byla úspěšně aktualizována" });
    }

    /// <summary>
    /// Aktualizuje kategorii výdaje (pouze pro Pending výdaje)
    /// </summary>
    [HttpPatch("{id}/category")]
    public async Task<IActionResult> UpdateExpenseCategory(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var expense = await _context.Expenses
            .Include(e => e.Workplace)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (expense == null)
        {
            return NotFound();
        }

        // Allow updating only for pending expenses
        if (expense.Status != ExpenseStatus.Pending)
        {
            return BadRequest(new { message = "Lze upravit pouze výdaje čekající na schválení" });
        }

        // Verify that category has an active limit for this workplace
        var hasLimit = await _context.WorkplaceLimits
            .AnyAsync(wl => wl.WorkplaceId == expense.WorkplaceId
                         && wl.CategoryId == request.CategoryId
                         && wl.IsActive);

        if (!hasLimit)
        {
            return BadRequest(new { message = "Vybraná kategorie nemá aktivní limit pro toto pracoviště" });
        }

        expense.CategoryId = request.CategoryId;
        expense.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Kategorie byla úspěšně aktualizována" });
    }

    /// <summary>
    /// Aktualizuje přílohy výdaje (pouze pro Pending výdaje)
    /// </summary>
    [HttpPatch("{id}/attachments")]
    public async Task<IActionResult> UpdateExpenseAttachments(Guid id, [FromBody] UpdateAttachmentsRequest request)
    {
        var expense = await _context.Expenses
            .Include(e => e.Attachments)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (expense == null)
        {
            return NotFound();
        }

        // Allow updating only for pending expenses
        if (expense.Status != ExpenseStatus.Pending)
        {
            return BadRequest(new { message = "Lze upravit pouze výdaje čekající na schválení" });
        }

        // Remove old attachments
        _context.ExpenseAttachments.RemoveRange(expense.Attachments);

        // Add new attachments
        if (request.Attachments != null && request.Attachments.Count > 0)
        {
            foreach (var attachmentDto in request.Attachments)
            {
                // Compress image
                var (compressedBase64, compressedSize) = await _imageCompressionService.CompressImageToBase64Async(
                    attachmentDto.Base64Data,
                    attachmentDto.FileType
                );

                var attachment = new ExpenseAttachment
                {
                    Id = Guid.NewGuid(),
                    ExpenseId = id,
                    OriginalFileName = attachmentDto.FileName,
                    DataType = "image/jpeg", // Always JPEG after compression
                    FileSize = compressedSize,
                    Base64Data = compressedBase64,
                    UploadedAt = DateTime.UtcNow
                };

                _context.ExpenseAttachments.Add(attachment);
            }
        }

        expense.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Přílohy byly úspěšně aktualizovány" });
    }

    /// <summary>
    /// Schválí výdaj
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveExpense(Guid id, [FromBody] ApprovalRequest? request = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
            {
                return NotFound(new { message = "Expense not found" });
            }

            expense.Status = ExpenseStatus.Approved;
            expense.LastDecisionAt = DateTime.UtcNow;
            expense.LastDecisionBy = userId;
            expense.UpdatedAt = DateTime.UtcNow;

            // Přidat záznam do historie schvalování
            var approval = new ExpenseApproval
            {
                Id = Guid.NewGuid(),
                ExpenseId = id,
                Action = ApprovalAction.Approved,
                ActorUserId = userId,
                Note = request?.Note,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.ExpenseApprovals.Add(approval);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Expense {ExpenseId} approved by {UserId}", id, userId);

            return Ok(new { message = "Expense approved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to approve expense {ExpenseId}", id);
            return StatusCode(500, new { message = "Failed to approve expense" });
        }
    }

    /// <summary>
    /// Zamítne výdaj
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectExpense(Guid id, [FromBody] ApprovalRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            if (string.IsNullOrWhiteSpace(request?.Note))
            {
                return BadRequest(new { message = "Rejection note is required" });
            }

            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
            {
                return NotFound(new { message = "Expense not found" });
            }

            expense.Status = ExpenseStatus.Rejected;
            expense.LastDecisionAt = DateTime.UtcNow;
            expense.LastDecisionBy = userId;
            expense.RejectionNote = request.Note;
            expense.UpdatedAt = DateTime.UtcNow;

            var approval = new ExpenseApproval
            {
                Id = Guid.NewGuid(),
                ExpenseId = id,
                Action = ApprovalAction.Rejected,
                ActorUserId = userId,
                Note = request.Note,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.ExpenseApprovals.Add(approval);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Expense {ExpenseId} rejected by {UserId}", id, userId);

            return Ok(new { message = "Expense rejected successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reject expense {ExpenseId}", id);
            return StatusCode(500, new { message = "Failed to reject expense" });
        }
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
