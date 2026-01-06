using CompanyExpenses.Database.Data;
using CompanyExpenses.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CompanyExpenses.Database.Repositories;

/// <summary>
/// Workplace repository interface
/// </summary>
public interface IWorkplaceRepository : IRepository<Workplace>
{
    Task<Workplace?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<Workplace>> GetAllWithMembersAsync();
    Task<WorkplaceDependencies> GetDependenciesAsync(Guid workplaceId);
}

/// <summary>
/// Workplace dependencies information
/// </summary>
public class WorkplaceDependencies
{
    public int MembersCount { get; set; }
    public int LimitsCount { get; set; }
    public int InvitationsCount { get; set; }
    public int ExpensesCount { get; set; }
    public bool CanDelete => MembersCount == 0 && LimitsCount == 0 && InvitationsCount == 0 && ExpensesCount == 0;
}

/// <summary>
/// Workplace repository implementation
/// </summary>
public class WorkplaceRepository : Repository<Workplace>, IWorkplaceRepository
{
    public WorkplaceRepository(AppDbContext context) : base(context) { }

    public async Task<Workplace?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(w => w.Members)
            .Include(w => w.Limits)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<IEnumerable<Workplace>> GetAllWithMembersAsync()
    {
        return await _dbSet
            .Include(w => w.Members)
            .ToListAsync();
    }

    public async Task<WorkplaceDependencies> GetDependenciesAsync(Guid workplaceId)
    {
        return new WorkplaceDependencies
        {
            MembersCount = await _context.WorkplaceMembers.CountAsync(m => m.WorkplaceId == workplaceId),
            LimitsCount = await _context.WorkplaceLimits.CountAsync(l => l.WorkplaceId == workplaceId),
            InvitationsCount = await _context.Invitations.CountAsync(i => i.WorkplaceId == workplaceId),
            ExpensesCount = await _context.Expenses.CountAsync(e => e.WorkplaceId == workplaceId)
        };
    }
}
