using CompanyExpenses.Database.Repositories;
using CompanyExpenses.Models.Entities;
using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;
using CompanyExpenses.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompanyExpenses.Services.Implementations;

/// <summary>
/// Expense category business service implementation
/// </summary>
public class ExpenseCategoryService : IExpenseCategoryService
{
    private readonly IExpenseCategoryRepository _categoryRepository;
    private readonly ILogger<ExpenseCategoryService> _logger;

    public ExpenseCategoryService(
        IExpenseCategoryRepository categoryRepository,
        ILogger<ExpenseCategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<IEnumerable<ExpenseCategoryDto>>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        var result = categories.Select(MapToDto);
        return ServiceResult<IEnumerable<ExpenseCategoryDto>>.Success(result);
    }

    public async Task<ServiceResult<IEnumerable<ExpenseCategoryDto>>> GetActiveCategoriesAsync()
    {
        var categories = await _categoryRepository.GetActiveAsync();
        var result = categories.Select(MapToDto);
        return ServiceResult<IEnumerable<ExpenseCategoryDto>>.Success(result);
    }

    public async Task<ServiceResult<ExpenseCategoryDto>> GetCategoryByIdAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return ServiceResult<ExpenseCategoryDto>.NotFound("Category not found");

        return ServiceResult<ExpenseCategoryDto>.Success(MapToDto(category));
    }

    public async Task<ServiceResult<ExpenseCategoryDto>> CreateCategoryAsync(CreateExpenseCategoryDto dto, string userId)
    {
        // Check name uniqueness
        if (!await _categoryRepository.IsNameUniqueAsync(dto.Name))
            return ServiceResult<ExpenseCategoryDto>.BadRequest("Category with this name already exists");

        var category = new ExpenseCategory
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Color = dto.Color,
            IsActive = dto.IsActive
        };

        await _categoryRepository.AddAsync(category);
        await _categoryRepository.SaveChangesAsync();

        _logger.LogInformation("Category created: {CategoryId} by user {UserId}", category.Id, userId);

        return ServiceResult<ExpenseCategoryDto>.Success(MapToDto(category));
    }

    public async Task<ServiceResult> UpdateCategoryAsync(Guid id, UpdateExpenseCategoryDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return ServiceResult.NotFound("Category not found");

        // Check name uniqueness (excluding current)
        if (!await _categoryRepository.IsNameUniqueAsync(dto.Name, id))
            return ServiceResult.BadRequest("Category with this name already exists");

        category.Name = dto.Name;
        category.Color = dto.Color;
        category.IsActive = dto.IsActive;

        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteCategoryAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return ServiceResult.NotFound("Category not found");

        _categoryRepository.Remove(category);
        await _categoryRepository.SaveChangesAsync();

        _logger.LogInformation("Category deleted: {CategoryId}", id);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> ActivateCategoryAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return ServiceResult.NotFound("Category not found");

        category.IsActive = true;
        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangesAsync();

        _logger.LogInformation("Category activated: {CategoryId}", id);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeactivateCategoryAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return ServiceResult.NotFound("Category not found");

        category.IsActive = false;
        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangesAsync();

        _logger.LogInformation("Category deactivated: {CategoryId}", id);
        return ServiceResult.Success();
    }

    private static ExpenseCategoryDto MapToDto(ExpenseCategory category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Color = category.Color,
        IsActive = category.IsActive
    };
}
