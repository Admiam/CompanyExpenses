using CompanyExpenses.Database.Repositories;
using CompanyExpenses.Models.Entities;
using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;
using CompanyExpenses.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompanyExpenses.Services.Implementations;

/// <summary>
/// Service implementation for expense category management including CRUD and activation operations.
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

    /// <summary>
    /// Retrieves all expense categories including inactive ones.
    /// </summary>
    /// <returns>A list of all categories.</returns>
    public async Task<ServiceResult<IEnumerable<ExpenseCategoryDto>>> GetAllCategoriesAsync()
    {
        _logger.LogInformation("Fetching all expense categories");
        var categories = await _categoryRepository.GetAllAsync();
        var result = categories.Select(MapToDto);
        return ServiceResult<IEnumerable<ExpenseCategoryDto>>.Success(result);
    }

    /// <summary>
    /// Retrieves only active expense categories.
    /// </summary>
    /// <returns>A list of active categories.</returns>
    public async Task<ServiceResult<IEnumerable<ExpenseCategoryDto>>> GetActiveCategoriesAsync()
    {
        _logger.LogInformation("Fetching active expense categories");
        var categories = await _categoryRepository.GetActiveAsync();
        var result = categories.Select(MapToDto);
        return ServiceResult<IEnumerable<ExpenseCategoryDto>>.Success(result);
    }

    /// <summary>
    /// Retrieves a specific category by ID.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <returns>The category if found, otherwise NotFound.</returns>
    public async Task<ServiceResult<ExpenseCategoryDto>> GetCategoryByIdAsync(Guid id)
    {
        _logger.LogInformation("Fetching category {CategoryId}", id);
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            _logger.LogWarning("Category not found: {CategoryId}", id);
            return ServiceResult<ExpenseCategoryDto>.NotFound("Category not found");
        }

        return ServiceResult<ExpenseCategoryDto>.Success(MapToDto(category));
    }

    /// <summary>
    /// Creates a new expense category with unique name validation.
    /// </summary>
    /// <param name="dto">The category creation data.</param>
    /// <param name="userId">The ID of the user creating the category.</param>
    /// <returns>The created category.</returns>
    public async Task<ServiceResult<ExpenseCategoryDto>> CreateCategoryAsync(CreateExpenseCategoryDto dto, string userId)
    {
        _logger.LogInformation("Creating expense category '{Name}' by user {UserId}", dto.Name, userId);

        if (!await _categoryRepository.IsNameUniqueAsync(dto.Name))
        {
            _logger.LogWarning("Category name already exists: {Name}", dto.Name);
            return ServiceResult<ExpenseCategoryDto>.BadRequest("Category with this name already exists");
        }

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

    /// <summary>
    /// Updates an existing category's properties.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <param name="dto">The updated category data.</param>
    /// <returns>Success or failure result.</returns>
    public async Task<ServiceResult> UpdateCategoryAsync(Guid id, UpdateExpenseCategoryDto dto)
    {
        _logger.LogInformation("Updating category {CategoryId}", id);
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            _logger.LogWarning("Category not found: {CategoryId}", id);
            return ServiceResult.NotFound("Category not found");
        }

        if (!await _categoryRepository.IsNameUniqueAsync(dto.Name, id))
        {
            _logger.LogWarning("Category name already exists: {Name}", dto.Name);
            return ServiceResult.BadRequest("Category with this name already exists");
        }

        category.Name = dto.Name;
        category.Color = dto.Color;
        category.IsActive = dto.IsActive;

        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangesAsync();

        _logger.LogInformation("Category updated successfully: {CategoryId}", id);
        return ServiceResult.Success();
    }

    /// <summary>
    /// Permanently deletes a category from the database.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <returns>Success or failure result.</returns>
    public async Task<ServiceResult> DeleteCategoryAsync(Guid id)
    {
        _logger.LogInformation("Deleting category {CategoryId}", id);
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            _logger.LogWarning("Category not found: {CategoryId}", id);
            return ServiceResult.NotFound("Category not found");
        }

        _categoryRepository.Remove(category);
        await _categoryRepository.SaveChangesAsync();

        _logger.LogInformation("Category deleted: {CategoryId}", id);
        return ServiceResult.Success();
    }

    /// <summary>
    /// Activates a previously deactivated category.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <returns>Success or failure result.</returns>
    public async Task<ServiceResult> ActivateCategoryAsync(Guid id)
    {
        _logger.LogInformation("Activating category {CategoryId}", id);
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            _logger.LogWarning("Category not found: {CategoryId}", id);
            return ServiceResult.NotFound("Category not found");
        }

        category.IsActive = true;
        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangesAsync();

        _logger.LogInformation("Category activated: {CategoryId}", id);
        return ServiceResult.Success();
    }

    /// <summary>
    /// Deactivates a category, making it unavailable for new expenses.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <returns>Success or failure result.</returns>
    public async Task<ServiceResult> DeactivateCategoryAsync(Guid id)
    {
        _logger.LogInformation("Deactivating category {CategoryId}", id);
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            _logger.LogWarning("Category not found: {CategoryId}", id);
            return ServiceResult.NotFound("Category not found");
        }

        category.IsActive = false;
        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangesAsync();

        _logger.LogInformation("Category deactivated: {CategoryId}", id);
        return ServiceResult.Success();
    }

    /// <summary>
    /// Maps an ExpenseCategory entity to its DTO representation.
    /// </summary>
    private static ExpenseCategoryDto MapToDto(ExpenseCategory category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Color = category.Color,
        IsActive = category.IsActive
    };
}
