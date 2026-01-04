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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var category = await _context.ExpenseCategories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
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
