using CompanyExpenses.Database.Data;
using CompanyExpenses.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CompanyExpenses.Database.Repositories;

/// <summary>
/// Workplace limit repository interface
/// </summary>
public interface IWorkplaceLimitRepository : IRepository<WorkplaceLimit>
{
    Task<IEnumerable<WorkplaceLimit>> GetByWorkplaceIdAsync(Guid workplaceId);
    Task<WorkplaceLimit?> GetActiveLimitAsync(Guid workplaceId, Guid categoryId);
    Task<bool> HasActiveLimitAsync(Guid workplaceId, Guid categoryId);
    Task<bool> HasOverlappingLimitAsync(Guid workplaceId, Guid? categoryId, DateOnly periodFrom, DateOnly periodTo, Guid? excludeLimitId = null);
    Task<decimal> GetUsedAmountAsync(Guid limitId);
}

/// <summary>
/// Workplace limit repository implementation
/// </summary>
public class WorkplaceLimitRepository : Repository<WorkplaceLimit>, IWorkplaceLimitRepository
{
    private new readonly AppDbContext _context;

    public WorkplaceLimitRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkplaceLimit>> GetByWorkplaceIdAsync(Guid workplaceId)
    {
        return await _dbSet
            .Include(l => l.Category)
            .Where(l => l.WorkplaceId == workplaceId)
            .ToListAsync();
    }

    public async Task<WorkplaceLimit?> GetActiveLimitAsync(Guid workplaceId, Guid categoryId)
    {
        return await _dbSet.FirstOrDefaultAsync(l =>
            l.WorkplaceId == workplaceId &&
            l.CategoryId == categoryId &&
            l.IsActive);
    }

    public async Task<bool> HasActiveLimitAsync(Guid workplaceId, Guid categoryId)
    {
        return await _dbSet.AnyAsync(l =>
            l.WorkplaceId == workplaceId &&
            l.CategoryId == categoryId &&
            l.IsActive);
    }

    public async Task<bool> HasOverlappingLimitAsync(Guid workplaceId, Guid? categoryId, DateOnly periodFrom, DateOnly periodTo, Guid? excludeLimitId = null)
    {
        var query = _dbSet.Where(l =>
            l.WorkplaceId == workplaceId &&
            l.CategoryId == categoryId &&
            l.IsActive &&
            l.PeriodFrom < periodTo &&
            l.PeriodTo > periodFrom);

        if (excludeLimitId.HasValue)
        {
            query = query.Where(l => l.Id != excludeLimitId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<decimal> GetUsedAmountAsync(Guid limitId)
    {
        var limit = await _dbSet
            .FirstOrDefaultAsync(l => l.Id == limitId);

        if (limit == null)
            return 0;

        // Get expenses within the limit's period and category
        var usedAmount = await _context.Expenses
            .Where(e => e.WorkplaceId == limit.WorkplaceId
                     && e.CategoryId == limit.CategoryId
                     && e.ExpenseDate >= limit.PeriodFrom
                     && e.ExpenseDate <= limit.PeriodTo
                     && !e.IsDeleted)
            .SumAsync(e => e.Amount);

        return usedAmount;
    }
}
