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

    /// <summary>
    /// Získá všechny aktivní kategorie výdajů
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseCategory>>> GetCategories()
    {
        return await _context.ExpenseCategories
            .Where(c => c.IsActive)
            .ToListAsync();
    }

    /// <summary>
    /// Získá kategorii podle ID
    /// </summary>
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

    /// <summary>
    /// Vytvoří novou kategorii výdajů
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ExpenseCategory>> CreateCategory(ExpenseCategory category)
    {
        category.Id = Guid.NewGuid();

        _context.ExpenseCategories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
    }

    /// <summary>
    /// Aktualizuje kategorii
    /// </summary>
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
    /// Deaktivuje kategorii (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
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
}
