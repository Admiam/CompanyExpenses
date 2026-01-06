using CompanyExpenses.Models.Entities;
using CompanyExpenses.Models.Enums;
using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;

namespace CompanyExpenses.Services.Interfaces;

/// <summary>
/// Expense business service interface
/// </summary>
public interface IExpenseService
{
    Task<ServiceResult<IEnumerable<ExpenseListDto>>> GetExpensesAsync(ExpenseFilterDto filter);
    Task<ServiceResult<ExpenseDetailDto>> GetExpenseByIdAsync(Guid id);
    Task<ServiceResult<ExpenseDto>> CreateExpenseAsync(CreateExpenseDto dto, string userId);
    Task<ServiceResult> UpdateExpenseAmountAsync(Guid id, decimal amount);
    Task<ServiceResult> UpdateExpenseCategoryAsync(Guid id, Guid categoryId);
    Task<ServiceResult> UpdateExpenseAttachmentsAsync(Guid id, List<AttachmentUploadDto>? attachments);
    Task<ServiceResult> ApproveExpenseAsync(Guid id, string userId, string? note);
    Task<ServiceResult> RejectExpenseAsync(Guid id, string userId, string note);
    Task<ServiceResult> DeleteExpenseAsync(Guid id);
    Task<ServiceResult<DashboardStatsDto>> GetDashboardStatsAsync();
}

/// <summary>
/// Interface for image compression service
/// </summary>
public interface IImageCompressionService
{
    Task<(string base64Data, long compressedSize)> CompressImageToBase64Async(string base64Input, string contentType);
}
