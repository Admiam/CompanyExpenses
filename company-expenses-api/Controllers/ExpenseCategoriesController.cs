using CompanyExpenses.Database.Data;
using CompanyExpenses.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CompanyExpenses.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpenseCategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ExpenseCategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseCategory>>> GetCategories()
    {
        return await _context.ExpenseCategories
            .ToListAsync();
    }

    /// <summary>
    /// Získá kategorie, které mají aktivní limit pro dané pracoviště
    /// </summary>
    [HttpGet("workplace/{workplaceId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetCategoriesForWorkplace(Guid workplaceId)
    {
        var categories = await _context.WorkplaceLimits
            .Include(wl => wl.Category)
            .Where(wl => wl.WorkplaceId == workplaceId && wl.IsActive)
            .Select(wl => new
            {
                id = wl.CategoryId,
                name = wl.Category!.Name,
                limitAmount = wl.LimitAmount,
                periodFrom = wl.PeriodFrom,
                periodTo = wl.PeriodTo
            })
            .Distinct()
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExpenseCategory>> GetCategory(Guid id)
    {
        var category = await _context.ExpenseCategories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        return category;
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseCategory>> CreateCategory(ExpenseCategory category)
    {
        category.Id = Guid.NewGuid();

        _context.ExpenseCategories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(Guid id, ExpenseCategory category)
    {
        if (id != category.Id)
        {
            return BadRequest();
        }

        _context.Entry(category).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.ExpenseCategories.AnyAsync(e => e.Id == id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Získá informace o závislostech kategorie (výdaje a limity)
    /// </summary>
    [HttpGet("{id}/dependencies")]
    public async Task<ActionResult<CategoryDependenciesDto>> GetCategoryDependencies(Guid id)
    {
        var category = await _context.ExpenseCategories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        var expensesCount = await _context.Expenses.CountAsync(e => e.CategoryId == id);
        var limitsCount = await _context.WorkplaceLimits.CountAsync(l => l.CategoryId == id);

        return Ok(new CategoryDependenciesDto
        {
            CategoryId = id,
            ExpensesCount = expensesCount,
            LimitsCount = limitsCount,
            CanDelete = expensesCount == 0 && limitsCount == 0
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var category = await _context.ExpenseCategories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        // Check dependencies
        var expensesCount = await _context.Expenses.CountAsync(e => e.CategoryId == id);
        var limitsCount = await _context.WorkplaceLimits.CountAsync(l => l.CategoryId == id);

        if (expensesCount > 0 || limitsCount > 0)
        {
            return BadRequest(new
            {
                message = "Cannot delete category with existing dependencies",
                dependencies = new
                {
                    expensesCount,
                    limitsCount
                }
            });
        }

        _context.ExpenseCategories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("deactivate/{id}")]
    public async Task<IActionResult> DeactivateCategory(Guid id)
    {
        var category = await _context.ExpenseCategories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        category.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("activate/{id}")]
    public async Task<IActionResult> ActivateCategory(Guid id)
    {
        var category = await _context.ExpenseCategories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        category.IsActive = true;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CategoryDependenciesDto
{
    public Guid CategoryId { get; set; }
    public int ExpensesCount { get; set; }
    public int LimitsCount { get; set; }
    public bool CanDelete { get; set; }
}
