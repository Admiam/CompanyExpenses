using CompanyExpenses.Database.Data;
using CompanyExpenses.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CompanyExpenses.Database.Repositories;

/// <summary>
/// Expense attachment repository interface
/// </summary>
public interface IExpenseAttachmentRepository : IRepository<ExpenseAttachment>
{
    Task<IEnumerable<ExpenseAttachment>> GetByExpenseIdAsync(Guid expenseId);
}

/// <summary>
/// Expense attachment repository implementation
/// </summary>
public class ExpenseAttachmentRepository : Repository<ExpenseAttachment>, IExpenseAttachmentRepository
{
    public ExpenseAttachmentRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<ExpenseAttachment>> GetByExpenseIdAsync(Guid expenseId)
    {
        return await _dbSet.Where(a => a.ExpenseId == expenseId).ToListAsync();
    }
}
