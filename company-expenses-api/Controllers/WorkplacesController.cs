using CompanyExpenses.Database.Data;
using CompanyExpenses.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CompanyExpenses.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkplacesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<WorkplacesController> _logger;

    public WorkplacesController(AppDbContext context, ILogger<WorkplacesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Získá seznam všech pracovišť
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Workplace>>> GetWorkplaces()
    {
        return await _context.Workplaces
            .Where(w => w.IsActive)
            .Include(w => w.Members)
            .ToListAsync();
    }

    /// <summary>
    /// Získá konkrétní pracoviště podle ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Workplace>> GetWorkplace(Guid id)
    {
        var workplace = await _context.Workplaces
            .Include(w => w.Members)
            .Include(w => w.Limits)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workplace == null)
        {
            return NotFound();
        }

        return workplace;
    }

    /// <summary>
    /// Vytvoří nové pracoviště
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Workplace>> CreateWorkplace(Workplace workplace)
    {
        workplace.Id = Guid.NewGuid();
        workplace.CreatedAt = DateTime.UtcNow;
        workplace.CreatedBy = "test-user"; // TODO: Získat z authentication

        _context.Workplaces.Add(workplace);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWorkplace), new { id = workplace.Id }, workplace);
    }

    /// <summary>
    /// Aktualizuje existující pracoviště
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWorkplace(Guid id, Workplace workplace)
    {
        if (id != workplace.Id)
        {
            return BadRequest();
        }

        _context.Entry(workplace).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await WorkplaceExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Smaže pracoviště (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkplace(Guid id)
    {
        var workplace = await _context.Workplaces.FindAsync(id);
        if (workplace == null)
        {
            return NotFound();
        }

        workplace.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> WorkplaceExists(Guid id)
    {
        return await _context.Workplaces.AnyAsync(e => e.Id == id);
    }
}
