using CompanyExpenses.Database.Data;
using CompanyExpenses.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CompanyExpenses.Database.Repositories;

/// <summary>
/// Workplace member repository interface
/// </summary>
public interface IWorkplaceMemberRepository : IRepository<WorkplaceMember>
{
    Task<IEnumerable<WorkplaceMember>> GetByWorkplaceIdAsync(Guid workplaceId);
    Task<IEnumerable<WorkplaceMember>> GetByUserIdAsync(string userId);
    Task<WorkplaceMember?> GetByUserAndWorkplaceAsync(string userId, Guid workplaceId);
    Task<IEnumerable<Workplace>> GetWorkplacesByUserIdAsync(string userId);
    Task<bool> IsMemberOfWorkplaceAsync(string userId, Guid workplaceId);
    Task<bool> IsManagerOfWorkplaceAsync(string userId, Guid workplaceId);
}

/// <summary>
/// Workplace member repository implementation
/// </summary>
public class WorkplaceMemberRepository : Repository<WorkplaceMember>, IWorkplaceMemberRepository
{
    public WorkplaceMemberRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<WorkplaceMember>> GetByWorkplaceIdAsync(Guid workplaceId)
    {
        return await _dbSet.Where(m => m.WorkplaceId == workplaceId).ToListAsync();
    }

    public async Task<IEnumerable<WorkplaceMember>> GetByUserIdAsync(string userId)
    {
        return await _dbSet.Where(m => m.UserId == userId).ToListAsync();
    }

    public async Task<bool> IsMemberOfWorkplaceAsync(string userId, Guid workplaceId)
    {
        return await _dbSet.AnyAsync(m => m.UserId == userId && m.WorkplaceId == workplaceId);
    }

    public async Task<bool> IsManagerOfWorkplaceAsync(string userId, Guid workplaceId)
    {
        return await _dbSet.AnyAsync(m => m.UserId == userId && m.WorkplaceId == workplaceId && m.IsManager);
    }

    public async Task<WorkplaceMember?> GetByUserAndWorkplaceAsync(string userId, Guid workplaceId)
    {
        return await _dbSet.FirstOrDefaultAsync(m => m.UserId == userId && m.WorkplaceId == workplaceId);
    }

    public async Task<IEnumerable<Workplace>> GetWorkplacesByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(m => m.UserId == userId)
            .Include(m => m.Workplace)
            .Select(m => m.Workplace!)
            .ToListAsync();
    }
}
