using CompanyExpenses.Database.Repositories;
using CompanyExpenses.Models.Entities;
using CompanyExpenses.Models.Enums;
using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;
using CompanyExpenses.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompanyExpenses.Services.Implementations;

/// <summary>
/// Service implementation for expense business logic including CRUD, approval workflows, and reporting.
/// </summary>
public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IExpenseApprovalRepository _approvalRepository;
    private readonly IExpenseAttachmentRepository _attachmentRepository;
    private readonly IWorkplaceLimitRepository _limitRepository;
    private readonly IWorkplaceRepository _workplaceRepository;
    private readonly IImageCompressionService _imageCompressionService;
    private readonly ILogger<ExpenseService> _logger;

    public ExpenseService(
        IExpenseRepository expenseRepository,
        IExpenseApprovalRepository approvalRepository,
        IExpenseAttachmentRepository attachmentRepository,
        IWorkplaceLimitRepository limitRepository,
        IWorkplaceRepository workplaceRepository,
        IImageCompressionService imageCompressionService,
        ILogger<ExpenseService> logger)
    {
        _expenseRepository = expenseRepository;
        _approvalRepository = approvalRepository;
        _attachmentRepository = attachmentRepository;
        _limitRepository = limitRepository;
        _workplaceRepository = workplaceRepository;
        _imageCompressionService = imageCompressionService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a filtered list of expenses based on the provided filter criteria.
    /// </summary>
    /// <param name="filter">Filter criteria including workplace, employee, and status.</param>
    /// <returns>A list of expenses matching the filter.</returns>
    public async Task<ServiceResult<IEnumerable<ExpenseListDto>>> GetExpensesAsync(ExpenseFilterDto filter)
    {
        _logger.LogInformation("Fetching expenses with filter - WorkplaceId: {WorkplaceId}, Status: {Status}",
            filter.WorkplaceId, filter.Status);
        // Parse status from string if provided
        ExpenseStatus? status = null;
        if (!string.IsNullOrEmpty(filter.Status) && Enum.TryParse<ExpenseStatus>(filter.Status, true, out var parsedStatus))
        {
            status = parsedStatus;
        }

        var expenses = await _expenseRepository.GetFilteredAsync(filter.WorkplaceId, filter.EmployeeUserId, status);

        var result = expenses.Select(e => new ExpenseListDto
        {
            Id = e.Id,
            Description = e.Description,
            Amount = e.Amount,
            Currency = e.Currency,
            ExpenseDate = e.ExpenseDate,
            Status = e.Status.ToString(),
            EmployeeUserId = e.EmployeeUserId,
            WorkplaceId = e.WorkplaceId,
            CategoryId = e.CategoryId,
            Workplace = e.Workplace != null ? new WorkplaceInfoDto { Id = e.Workplace.Id, Name = e.Workplace.Name } : null!,
            Category = e.Category != null ? new CategoryInfoDto { Id = e.Category.Id, Name = e.Category.Name } : null!,
            SubmittedAt = e.SubmittedAt,
            CreatedAt = e.CreatedAt
        });

        return ServiceResult<IEnumerable<ExpenseListDto>>.Success(result);
    }

    /// <summary>
    /// Retrieves detailed information about a specific expense including attachments and approvals.
    /// </summary>
    /// <param name="id">The unique identifier of the expense.</param>
    /// <returns>The expense details if found, otherwise NotFound.</returns>
    public async Task<ServiceResult<ExpenseDetailDto>> GetExpenseByIdAsync(Guid id)
    {
        _logger.LogInformation("Fetching expense details for ID: {ExpenseId}", id);
        var expense = await _expenseRepository.GetByIdWithDetailsAsync(id);
        if (expense == null)
        {
            _logger.LogWarning("Expense not found: {ExpenseId}", id);
            return ServiceResult<ExpenseDetailDto>.NotFound("Expense not found");
        }

        var result = new ExpenseDetailDto
        {
            Id = expense.Id,
            Description = expense.Description,
            Amount = expense.Amount,
            Currency = expense.Currency,
            ExpenseDate = expense.ExpenseDate,
            Status = expense.Status.ToString(),
            EmployeeUserId = expense.EmployeeUserId,
            WorkplaceId = expense.WorkplaceId,
            CategoryId = expense.CategoryId,
            Workplace = expense.Workplace != null ? new WorkplaceInfoDto { Id = expense.Workplace.Id, Name = expense.Workplace.Name } : null!,
            Category = expense.Category != null ? new CategoryInfoDto { Id = expense.Category.Id, Name = expense.Category.Name } : null!,
            SubmittedAt = expense.SubmittedAt,
            CreatedAt = expense.CreatedAt,
            LastDecisionAt = expense.LastDecisionAt,
            LastDecisionBy = expense.LastDecisionBy,
            RejectionNote = expense.RejectionNote,
            Attachments = expense.Attachments.Select(a => new ExpenseAttachmentDto
            {
                Id = a.Id,
                OriginalFileName = a.OriginalFileName,
                DataType = a.DataType,
                FileSize = a.FileSize,
                Base64Data = a.Base64Data,
                UploadedAt = a.UploadedAt
            }).ToList(),
            Approvals = expense.Approvals.Select(a => new ExpenseApprovalDto
            {
                Id = a.Id,
                Action = a.Action.ToString(),
                ActorEmail = a.ActorUserId, // Will be resolved in controller with user lookup
                Note = a.Note,
                CreatedAt = a.CreatedAt
            }).OrderByDescending(a => a.CreatedAt).ToList()
        };

        return ServiceResult<ExpenseDetailDto>.Success(result);
    }

    /// <summary>
    /// Creates a new expense with optional attachments. Images are compressed before storage.
    /// </summary>
    /// <param name="dto">The expense data including attachments.</param>
    /// <param name="userId">The ID of the user creating the expense.</param>
    /// <returns>The created expense details.</returns>
    public async Task<ServiceResult<ExpenseDto>> CreateExpenseAsync(CreateExpenseDto dto, string userId)
    {
        _logger.LogInformation("Creating expense for user {UserId} in workplace {WorkplaceId}", userId, dto.WorkplaceId);
        try
        {
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

            await _expenseRepository.AddAsync(expense);

            // Process attachments
            int attachmentsCount = 0;
            if (dto.Attachments != null && dto.Attachments.Any())
            {
                foreach (var attachmentDto in dto.Attachments)
                {
                    try
                    {
                        var (compressedBase64, compressedSize) = await _imageCompressionService
                            .CompressImageToBase64Async(attachmentDto.Base64Data, attachmentDto.FileType);

                        var attachment = new ExpenseAttachment
                        {
                            Id = Guid.NewGuid(),
                            ExpenseId = expense.Id,
                            OriginalFileName = attachmentDto.FileName,
                            StoredFileName = $"{Guid.NewGuid()}{Path.GetExtension(attachmentDto.FileName)}",
                            DataType = "image/jpeg",
                            FileSize = compressedSize,
                            Base64Data = compressedBase64,
                            UploadedByUserId = userId,
                            UploadedAt = DateTime.UtcNow
                        };

                        await _attachmentRepository.AddAsync(attachment);
                        attachmentsCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to compress attachment: {FileName}", attachmentDto.FileName);
                    }
                }
            }

            await _expenseRepository.SaveChangesAsync();

            _logger.LogInformation("Expense created: {ExpenseId} by user {UserId}", expense.Id, userId);

            return ServiceResult<ExpenseDto>.Success(new ExpenseDto
            {
                Id = expense.Id,
                Description = expense.Description,
                Amount = expense.Amount,
                Currency = expense.Currency,
                ExpenseDate = expense.ExpenseDate,
                Status = expense.Status.ToString(),
                AttachmentsCount = attachmentsCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create expense");
            return ServiceResult<ExpenseDto>.Error("Failed to create expense");
        }
    }

    /// <summary>
    /// Updates the amount of a pending expense. Only pending expenses can be modified.
    /// </summary>
    /// <param name="id">The expense ID.</param>
    /// <param name="amount">The new amount value.</param>
    /// <returns>Success or failure result.</returns>
    public async Task<ServiceResult> UpdateExpenseAmountAsync(Guid id, decimal amount)
    {
        _logger.LogInformation("Updating expense amount for {ExpenseId} to {Amount}", id, amount);
        var expense = await _expenseRepository.GetByIdAsync(id);
        if (expense == null)
        {
            _logger.LogWarning("Expense not found: {ExpenseId}", id);
            return ServiceResult.NotFound("Expense not found");
        }

        if (expense.Status != ExpenseStatus.Pending)
        {
            _logger.LogWarning("Cannot update non-pending expense {ExpenseId}, status: {Status}", id, expense.Status);
            return ServiceResult.BadRequest("Can only update pending expenses");
        }

        expense.Amount = amount;
        expense.UpdatedAt = DateTime.UtcNow;

        await _expenseRepository.SaveChangesAsync();
        _logger.LogInformation("Expense amount updated successfully: {ExpenseId}", id);
        return ServiceResult.Success();
    }

    /// <summary>
    /// Updates the category of a pending expense. Validates that the category has an active limit.
    /// </summary>
    /// <param name="id">The expense ID.</param>
    /// <param name="categoryId">The new category ID.</param>
    /// <returns>Success or failure result.</returns>
    public async Task<ServiceResult> UpdateExpenseCategoryAsync(Guid id, Guid categoryId)
    {
        _logger.LogInformation("Updating expense category for {ExpenseId} to {CategoryId}", id, categoryId);
        var expense = await _expenseRepository.GetByIdAsync(id);
        if (expense == null)
        {
            _logger.LogWarning("Expense not found: {ExpenseId}", id);
            return ServiceResult.NotFound("Expense not found");
        }

        if (expense.Status != ExpenseStatus.Pending)
        {
            _logger.LogWarning("Cannot update non-pending expense {ExpenseId}", id);
            return ServiceResult.BadRequest("Can only update pending expenses");
        }

        // Verify category has active limit for workplace
        var hasLimit = await _limitRepository.HasActiveLimitAsync(expense.WorkplaceId, categoryId);
        if (!hasLimit)
            return ServiceResult.BadRequest("Selected category does not have an active limit for this workplace");

        expense.CategoryId = categoryId;
        expense.UpdatedAt = DateTime.UtcNow;

        await _expenseRepository.SaveChangesAsync();
        _logger.LogInformation("Expense category updated successfully: {ExpenseId}", id);
        return ServiceResult.Success();
    }

    /// <summary>
    /// Replaces all attachments on a pending expense. Old attachments are removed and new ones are compressed.
    /// </summary>
    /// <param name="id">The expense ID.</param>
    /// <param name="attachments">The new attachments to add.</param>
    /// <returns>Success or failure result.</returns>
    public async Task<ServiceResult> UpdateExpenseAttachmentsAsync(Guid id, List<AttachmentUploadDto>? attachments)
    {
        _logger.LogInformation("Updating attachments for expense {ExpenseId}", id);
        var expense = await _expenseRepository.GetByIdWithDetailsAsync(id);
        if (expense == null)
        {
            _logger.LogWarning("Expense not found: {ExpenseId}", id);
            return ServiceResult.NotFound("Expense not found");
        }

        if (expense.Status != ExpenseStatus.Pending)
        {
            _logger.LogWarning("Cannot update non-pending expense {ExpenseId}", id);
            return ServiceResult.BadRequest("Can only update pending expenses");
        }

        // Remove old attachments
        foreach (var attachment in expense.Attachments.ToList())
        {
            _attachmentRepository.Remove(attachment);
        }

        // Add new attachments
        if (attachments != null && attachments.Any())
        {
            foreach (var attachmentDto in attachments)
            {
                try
                {
                    var (compressedBase64, compressedSize) = await _imageCompressionService
                        .CompressImageToBase64Async(attachmentDto.Base64Data, attachmentDto.FileType);

                    var attachment = new ExpenseAttachment
                    {
                        Id = Guid.NewGuid(),
                        ExpenseId = id,
                        OriginalFileName = attachmentDto.FileName,
                        DataType = "image/jpeg",
                        FileSize = compressedSize,
                        Base64Data = compressedBase64,
                        UploadedAt = DateTime.UtcNow
                    };

                    await _attachmentRepository.AddAsync(attachment);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to compress attachment: {FileName}", attachmentDto.FileName);
                }
            }
        }

        expense.UpdatedAt = DateTime.UtcNow;
        await _expenseRepository.SaveChangesAsync();
        _logger.LogInformation("Expense attachments updated successfully: {ExpenseId}", id);
        return ServiceResult.Success();
    }

    /// <summary>
    /// Approves an expense and records the approval action in the history.
    /// </summary>
    /// <param name="id">The expense ID.</param>
    /// <param name="userId">The ID of the approving user.</param>
    /// <param name="note">Optional approval note.</param>
    /// <returns>Success or failure result.</returns>
    public async Task<ServiceResult> ApproveExpenseAsync(Guid id, string userId, string? note)
    {
        _logger.LogInformation("Approving expense {ExpenseId} by user {UserId}", id, userId);
        try
        {
            var expense = await _expenseRepository.GetByIdAsync(id);
            if (expense == null)
            {
                _logger.LogWarning("Expense not found for approval: {ExpenseId}", id);
                return ServiceResult.NotFound("Expense not found");
            }

            expense.Status = ExpenseStatus.Approved;
            expense.LastDecisionAt = DateTime.UtcNow;
            expense.LastDecisionBy = userId;
            expense.UpdatedAt = DateTime.UtcNow;

            var approval = new ExpenseApproval
            {
                Id = Guid.NewGuid(),
                ExpenseId = id,
                Action = ApprovalAction.Approved,
                ActorUserId = userId,
                Note = note,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _approvalRepository.AddAsync(approval);
            await _expenseRepository.SaveChangesAsync();

            _logger.LogInformation("Expense {ExpenseId} approved by {UserId}", id, userId);
            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to approve expense {ExpenseId}", id);
            return ServiceResult.Error("Failed to approve expense");
        }
    }

    /// <summary>
    /// Rejects an expense with a required rejection note. Records the rejection in the history.
    /// </summary>
    /// <param name="id">The expense ID.</param>
    /// <param name="userId">The ID of the rejecting user.</param>
    /// <param name="note">Required rejection reason.</param>
    /// <returns>Success or failure result.</returns>
    public async Task<ServiceResult> RejectExpenseAsync(Guid id, string userId, string note)
    {
        _logger.LogInformation("Rejecting expense {ExpenseId} by user {UserId}", id, userId);
        try
        {
            if (string.IsNullOrWhiteSpace(note))
            {
                _logger.LogWarning("Rejection note required for expense {ExpenseId}", id);
                return ServiceResult.BadRequest("Rejection note is required");
            }

            var expense = await _expenseRepository.GetByIdAsync(id);
            if (expense == null)
            {
                _logger.LogWarning("Expense not found for rejection: {ExpenseId}", id);
                return ServiceResult.NotFound("Expense not found");
            }

            expense.Status = ExpenseStatus.Rejected;
            expense.LastDecisionAt = DateTime.UtcNow;
            expense.LastDecisionBy = userId;
            expense.RejectionNote = note;
            expense.UpdatedAt = DateTime.UtcNow;

            var approval = new ExpenseApproval
            {
                Id = Guid.NewGuid(),
                ExpenseId = id,
                Action = ApprovalAction.Rejected,
                ActorUserId = userId,
                Note = note,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _approvalRepository.AddAsync(approval);
            await _expenseRepository.SaveChangesAsync();

            _logger.LogInformation("Expense {ExpenseId} rejected by {UserId}", id, userId);
            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reject expense {ExpenseId}", id);
            return ServiceResult.Error("Failed to reject expense");
        }
    }

    /// <summary>
    /// Soft-deletes an expense by marking it as deleted.
    /// </summary>
    /// <param name="id">The expense ID.</param>
    /// <returns>Success or failure result.</returns>
    public async Task<ServiceResult> DeleteExpenseAsync(Guid id)
    {
        _logger.LogInformation("Deleting expense {ExpenseId}", id);
        var expense = await _expenseRepository.GetByIdAsync(id);
        if (expense == null)
        {
            _logger.LogWarning("Expense not found for deletion: {ExpenseId}", id);
            return ServiceResult.NotFound("Expense not found");
        }

        expense.IsDeleted = true;
        expense.UpdatedAt = DateTime.UtcNow;

        await _expenseRepository.SaveChangesAsync();
        _logger.LogInformation("Expense deleted successfully: {ExpenseId}", id);
        return ServiceResult.Success();
    }

    /// <summary>
    /// Retrieves dashboard statistics including totals, monthly comparisons, and charts data.
    /// </summary>
    /// <returns>Dashboard statistics DTO.</returns>
    public async Task<ServiceResult<DashboardStatsDto>> GetDashboardStatsAsync()
    {
        _logger.LogInformation("Fetching dashboard statistics");
        var now = DateTime.UtcNow;
        var startOfMonth = DateOnly.FromDateTime(new DateTime(now.Year, now.Month, 1));
        var startOfLastMonth = startOfMonth.AddMonths(-1);
        var startOfYear = DateOnly.FromDateTime(new DateTime(now.Year, 1, 1));
        var today = DateOnly.FromDateTime(now);

        var totalExpenses = await _expenseRepository.GetTotalByStatusAsync(ExpenseStatus.Approved);
        var monthlyExpenses = await _expenseRepository.GetTotalByStatusAsync(ExpenseStatus.Approved, startOfMonth, today);
        var lastMonthExpenses = await _expenseRepository.GetTotalByStatusAsync(ExpenseStatus.Approved, startOfLastMonth, startOfMonth.AddDays(-1));

        var monthlyChange = lastMonthExpenses > 0
            ? ((monthlyExpenses - lastMonthExpenses) / lastMonthExpenses) * 100
            : 0;

        var workplacesCount = await _workplaceRepository.CountAsync(w => w.IsActive);
        var pendingExpensesCount = await _expenseRepository.CountAsync(e => e.Status == ExpenseStatus.Pending && !e.IsDeleted);

        var expensesByCategory = await _expenseRepository.GetExpensesByCategoryAsync(startOfYear);
        var expensesByWorkplace = await _expenseRepository.GetExpensesByWorkplaceAsync(startOfYear);

        var recentExpenses = (await _expenseRepository.GetRecentAsync(10))
            .Select(e => new
            {
                id = e.Id,
                description = e.Description,
                amount = e.Amount,
                currency = e.Currency,
                expenseDate = e.ExpenseDate,
                status = e.Status.ToString(),
                employeeUserId = e.EmployeeUserId,
                categoryName = e.Category?.Name,
                workplaceName = e.Workplace?.Name,
                submittedAt = e.SubmittedAt
            });

        return ServiceResult<DashboardStatsDto>.Success(new DashboardStatsDto
        {
            TotalExpenses = totalExpenses,
            MonthlyExpenses = monthlyExpenses,
            MonthlyChange = Math.Round(monthlyChange, 1),
            WorkplacesCount = workplacesCount,
            UsersCount = 0, // Will be set in controller from auth context
            PendingExpensesCount = pendingExpensesCount,
            ExpensesByCategory = expensesByCategory,
            ExpensesByWorkplace = expensesByWorkplace,
            RecentExpenses = recentExpenses.Cast<object>()
        });
    }
}
