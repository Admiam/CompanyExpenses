using CompanyExpenses.Database.Data;
using CompanyExpenses.Models.Entities;
using CompanyExpenses.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CompanyExpenses.Database.Repositories;

/// <summary>
/// Expense repository interface with specific expense operations
/// </summary>
public interface IExpenseRepository : IRepository<Expense>
{
    Task<Expense?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<Expense>> GetFilteredAsync(Guid? workplaceId, string? employeeUserId, ExpenseStatus? status);
    Task<decimal> GetTotalByStatusAsync(ExpenseStatus status, DateOnly? fromDate = null, DateOnly? toDate = null);
    Task<IEnumerable<Expense>> GetRecentAsync(int count);
    Task<IEnumerable<object>> GetExpensesByCategoryAsync(DateOnly fromDate);
    Task<IEnumerable<object>> GetExpensesByWorkplaceAsync(DateOnly fromDate);
}

/// <summary>
/// Expense repository implementation
/// </summary>
public class ExpenseRepository : Repository<Expense>, IExpenseRepository
{
    public ExpenseRepository(AppDbContext context) : base(context) { }

    public async Task<Expense?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(e => e.Category)
            .Include(e => e.Workplace)
            .Include(e => e.Approvals)
            .Include(e => e.Attachments)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Expense>> GetFilteredAsync(Guid? workplaceId, string? employeeUserId, ExpenseStatus? status)
    {
        var query = _dbSet
            .Include(e => e.Category)
            .Include(e => e.Workplace)
            .AsQueryable();

        if (workplaceId.HasValue)
            query = query.Where(e => e.WorkplaceId == workplaceId.Value);

        if (!string.IsNullOrEmpty(employeeUserId))
            query = query.Where(e => e.EmployeeUserId == employeeUserId);

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);

        return await query.OrderByDescending(e => e.ExpenseDate).ToListAsync();
    }

    public async Task<decimal> GetTotalByStatusAsync(ExpenseStatus status, DateOnly? fromDate = null, DateOnly? toDate = null)
    {
        var query = _dbSet.Where(e => e.Status == status && !e.IsDeleted);

        if (fromDate.HasValue)
            query = query.Where(e => e.ExpenseDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(e => e.ExpenseDate <= toDate.Value);

        return await query.SumAsync(e => e.Amount);
    }

    public async Task<IEnumerable<Expense>> GetRecentAsync(int count)
    {
        return await _dbSet
            .Include(e => e.Category)
            .Include(e => e.Workplace)
            .Where(e => !e.IsDeleted)
            .OrderByDescending(e => e.SubmittedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<object>> GetExpensesByCategoryAsync(DateOnly fromDate)
    {
        return await _dbSet
            .Include(e => e.Category)
            .Where(e => e.ExpenseDate >= fromDate &&
                       e.Status == ExpenseStatus.Approved &&
                       !e.IsDeleted &&
                       e.Category != null)
            .GroupBy(e => new { e.CategoryId, e.Category!.Name, e.Category.Color })
            .Select(g => new
            {
                categoryId = g.Key.CategoryId,
                categoryName = g.Key.Name,
                categoryColor = g.Key.Color,
                total = g.Sum(e => e.Amount),
                count = g.Count()
            })
            .OrderByDescending(x => x.total)
            .ToListAsync<object>();
    }

    public async Task<IEnumerable<object>> GetExpensesByWorkplaceAsync(DateOnly fromDate)
    {
        // First, get expenses grouped by workplace
        var workplaces = await _dbSet
            .Include(e => e.Workplace)
            .Include(e => e.Category)
            .Where(e => e.ExpenseDate >= fromDate &&
                       e.Status == ExpenseStatus.Approved &&
                       !e.IsDeleted &&
                       e.Workplace != null)
            .GroupBy(e => new { e.WorkplaceId, e.Workplace!.Name })
            .Select(g => new
            {
                workplaceId = g.Key.WorkplaceId,
                workplaceName = g.Key.Name,
                total = g.Sum(e => e.Amount),
                count = g.Count(),
                // Get categories breakdown for each workplace
                categories = g
                    .Where(e => e.Category != null)
                    .GroupBy(e => new { e.CategoryId, e.Category!.Name, e.Category.Color })
                    .Select(cg => new
                    {
                        categoryId = cg.Key.CategoryId,
                        categoryName = cg.Key.Name,
                        categoryColor = cg.Key.Color,
                        total = cg.Sum(e => e.Amount)
                    })
                    .ToList()
            })
            .OrderByDescending(x => x.total)
            .ToListAsync<object>();

        return workplaces;
    }
}
