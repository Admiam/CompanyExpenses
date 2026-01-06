using CompanyExpenses.Database.Data;
using CompanyExpenses.Models.Entities;
using CompanyExpenses.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CompanyExpenses.Database.Repositories;

/// <summary>
/// Invitation repository interface
/// </summary>
public interface IInvitationRepository : IRepository<Invitation>
{
    Task<Invitation?> GetByIdWithWorkplaceAsync(Guid id);
    Task<Invitation?> GetByTokenAsync(string token);
    Task<IEnumerable<Invitation>> GetAllWithWorkplaceAsync();
    Task<bool> HasPendingInvitationAsync(string email);
}

/// <summary>
/// Invitation repository implementation
/// </summary>
public class InvitationRepository : Repository<Invitation>, IInvitationRepository
{
    public InvitationRepository(AppDbContext context) : base(context) { }

    public async Task<Invitation?> GetByIdWithWorkplaceAsync(Guid id)
    {
        return await _dbSet
            .Include(i => i.Workplace)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Invitation?> GetByTokenAsync(string token)
    {
        return await _dbSet
            .Include(i => i.Workplace)
            .FirstOrDefaultAsync(i => i.Token == token);
    }

    public async Task<IEnumerable<Invitation>> GetAllWithWorkplaceAsync()
    {
        return await _dbSet
            .Include(i => i.Workplace)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> HasPendingInvitationAsync(string email)
    {
        return await _dbSet.AnyAsync(i => i.Email == email && i.Status == InvitationStatus.Pending);
    }
}
