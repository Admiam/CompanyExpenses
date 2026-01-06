using CompanyExpenses.Database.Data;
using CompanyExpenses.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CompanyExpenses.Database.Repositories;

/// <summary>
/// Expense category repository interface
/// </summary>
public interface IExpenseCategoryRepository : IRepository<ExpenseCategory>
{
    Task<IEnumerable<ExpenseCategory>> GetActiveAsync();
    Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null);
}

/// <summary>
/// Expense category repository implementation
/// </summary>
public class ExpenseCategoryRepository : Repository<ExpenseCategory>, IExpenseCategoryRepository
{
    public ExpenseCategoryRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<ExpenseCategory>> GetActiveAsync()
    {
        return await _dbSet.Where(c => c.IsActive).ToListAsync();
    }

    public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null)
    {
        var query = _dbSet.Where(c => c.Name == name);
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return !await query.AnyAsync();
    }
}
