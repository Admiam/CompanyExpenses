using CompanyExpenses.Database.Data;
using CompanyExpenses.Models.Entities;

namespace CompanyExpenses.Database.Repositories;

/// <summary>
/// Expense approval repository interface
/// </summary>
public interface IExpenseApprovalRepository : IRepository<ExpenseApproval>
{
    Task<IEnumerable<ExpenseApproval>> GetByExpenseIdAsync(Guid expenseId);
}

/// <summary>
/// Expense approval repository implementation
/// </summary>
public class ExpenseApprovalRepository : Repository<ExpenseApproval>, IExpenseApprovalRepository
{
    public ExpenseApprovalRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<ExpenseApproval>> GetByExpenseIdAsync(Guid expenseId)
    {
        return await FindAsync(a => a.ExpenseId == expenseId);
    }
}
