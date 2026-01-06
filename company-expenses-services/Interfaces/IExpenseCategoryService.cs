using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;

namespace CompanyExpenses.Services.Interfaces;

/// <summary>
/// Expense category business service interface
/// </summary>
public interface IExpenseCategoryService
{
    Task<ServiceResult<IEnumerable<ExpenseCategoryDto>>> GetAllCategoriesAsync();
    Task<ServiceResult<IEnumerable<ExpenseCategoryDto>>> GetActiveCategoriesAsync();
    Task<ServiceResult<ExpenseCategoryDto>> GetCategoryByIdAsync(Guid id);
    Task<ServiceResult<ExpenseCategoryDto>> CreateCategoryAsync(CreateExpenseCategoryDto dto, string userId);
    Task<ServiceResult> UpdateCategoryAsync(Guid id, UpdateExpenseCategoryDto dto);
    Task<ServiceResult> DeleteCategoryAsync(Guid id);
    Task<ServiceResult> ActivateCategoryAsync(Guid id);
    Task<ServiceResult> DeactivateCategoryAsync(Guid id);
}
