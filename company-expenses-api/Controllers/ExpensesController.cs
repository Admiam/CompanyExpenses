using CompanyExpenses.Database.Data;
using CompanyExpenses.Models.Entities;
using CompanyExpenses.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CompanyExpenses.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ExpensesController> _logger;

    public ExpensesController(AppDbContext context, ILogger<ExpensesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Získá seznam všech výdajů (s filtrováním)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Expense>>> GetExpenses(
        [FromQuery] Guid? workplaceId = null,
        [FromQuery] string? employeeUserId = null,
        [FromQuery] ExpenseStatus? status = null)
    {
        var query = _context.Expenses
            .Include(e => e.Category)
            .Include(e => e.Workplace)
            .Include(e => e.Attachments)
            .AsQueryable();

        if (workplaceId.HasValue)
        {
            query = query.Where(e => e.WorkplaceId == workplaceId.Value);
        }

        if (!string.IsNullOrEmpty(employeeUserId))
        {
            query = query.Where(e => e.EmployeeUserId == employeeUserId);
        }

        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }

        return await query
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();
    }

    /// <summary>
    /// Získá konkrétní výdaj podle ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Expense>> GetExpense(Guid id)
    {
        var expense = await _context.Expenses
            .Include(e => e.Category)
            .Include(e => e.Workplace)
            .Include(e => e.Attachments)
            .Include(e => e.Approvals)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (expense == null)
        {
            return NotFound();
        }

        return expense;
    }

    /// <summary>
    /// Vytvoří nový výdaj
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Expense>> CreateExpense(Expense expense)
    {
        expense.Id = Guid.NewGuid();
        expense.CreatedAt = DateTime.UtcNow;
        expense.SubmittedAt = DateTime.UtcNow;
        expense.Status = ExpenseStatus.Pending;
        expense.CreatedBy = "test-user"; // TODO: Získat z authentication
        expense.EmployeeUserId = "test-user"; // TODO: Získat z authentication

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, expense);
    }

    /// <summary>
    /// Schválí výdaj
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveExpense(Guid id, [FromBody] string? note = null)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense == null)
        {
            return NotFound();
        }

        expense.Status = ExpenseStatus.Approved;
        expense.LastDecisionAt = DateTime.UtcNow;
        expense.LastDecisionBy = "test-manager"; // TODO: Získat z authentication
        expense.UpdatedAt = DateTime.UtcNow;

        // Přidat záznam do historie schvalování
        var approval = new ExpenseApproval
        {
            Id = Guid.NewGuid(),
            ExpenseId = id,
            Action = ApprovalAction.Approved,
            ActorUserId = "test-manager",
            Note = note,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-manager"
        };

        _context.ExpenseApprovals.Add(approval);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Zamítne výdaj
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectExpense(Guid id, [FromBody] string rejectionNote)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense == null)
        {
            return NotFound();
        }

        expense.Status = ExpenseStatus.Rejected;
        expense.LastDecisionAt = DateTime.UtcNow;
        expense.LastDecisionBy = "test-manager";
        expense.RejectionNote = rejectionNote;
        expense.UpdatedAt = DateTime.UtcNow;

        var approval = new ExpenseApproval
        {
            Id = Guid.NewGuid(),
            ExpenseId = id,
            Action = ApprovalAction.Rejected,
            ActorUserId = "test-manager",
            Note = rejectionNote,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-manager"
        };

        _context.ExpenseApprovals.Add(approval);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Smaže výdaj (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(Guid id)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense == null)
        {
            return NotFound();
        }

        expense.IsDeleted = true;
        expense.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
