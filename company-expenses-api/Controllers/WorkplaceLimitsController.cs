using CompanyExpenses.Database.Data;
using CompanyExpenses.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CompanyExpenses.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkplaceLimitsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<WorkplaceLimitsController> _logger;

    public WorkplaceLimitsController(AppDbContext context, ILogger<WorkplaceLimitsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("workplace/{workplaceId}")]
    public async Task<ActionResult<IEnumerable<WorkplaceLimit>>> GetWorkplaceLimits(Guid workplaceId)
    {
        var limits = await _context.WorkplaceLimits
            .Include(wl => wl.Category)
            .Where(wl => wl.WorkplaceId == workplaceId && wl.IsActive)
            .OrderBy(wl => wl.PeriodFrom)
            .ToListAsync();

        return Ok(limits);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkplaceLimit>> GetLimit(Guid id)
    {
        var limit = await _context.WorkplaceLimits
            .Include(wl => wl.Category)
            .FirstOrDefaultAsync(wl => wl.Id == id);

        if (limit == null)
        {
            return NotFound();
        }

        return Ok(limit);
    }

    [HttpPost]
    public async Task<ActionResult<WorkplaceLimit>> CreateLimit(WorkplaceLimit limit)
    {
        limit.Id = Guid.NewGuid();
        limit.CreatedAt = DateTime.UtcNow;
        limit.UpdatedAt = DateTime.UtcNow;
        limit.IsActive = true;

        // Validate workplace exists
        var workplaceExists = await _context.Workplaces.AnyAsync(w => w.Id == limit.WorkplaceId);
        if (!workplaceExists)
        {
            return BadRequest("Workplace not found");
        }

        // Validate category if provided
        if (limit.CategoryId.HasValue)
        {
            var categoryExists = await _context.ExpenseCategories.AnyAsync(c => c.Id == limit.CategoryId.Value);
            if (!categoryExists)
            {
                return BadRequest("Category not found");
            }
        }

        // Check for overlapping periods
        var hasOverlap = await _context.WorkplaceLimits
            .Where(wl => wl.WorkplaceId == limit.WorkplaceId
                && wl.CategoryId == limit.CategoryId
                && wl.IsActive
                && wl.Id != limit.Id)
            .AnyAsync(wl =>
                (limit.PeriodFrom >= wl.PeriodFrom && limit.PeriodFrom <= wl.PeriodTo) ||
                (limit.PeriodTo >= wl.PeriodFrom && limit.PeriodTo <= wl.PeriodTo) ||
                (limit.PeriodFrom <= wl.PeriodFrom && limit.PeriodTo >= wl.PeriodTo));

        if (hasOverlap)
        {
            return BadRequest("A limit with overlapping period already exists for this workplace and category");
        }

        _context.WorkplaceLimits.Add(limit);
        await _context.SaveChangesAsync();

        // Reload with includes
        var createdLimit = await _context.WorkplaceLimits
            .Include(wl => wl.Category)
            .FirstOrDefaultAsync(wl => wl.Id == limit.Id);

        return CreatedAtAction(nameof(GetLimit), new { id = limit.Id }, createdLimit);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLimit(Guid id, WorkplaceLimit limit)
    {
        if (id != limit.Id)
        {
            return BadRequest("ID mismatch");
        }

        var existingLimit = await _context.WorkplaceLimits.FindAsync(id);
        if (existingLimit == null)
        {
            return NotFound();
        }

        // Validate category if provided
        if (limit.CategoryId.HasValue)
        {
            var categoryExists = await _context.ExpenseCategories.AnyAsync(c => c.Id == limit.CategoryId.Value);
            if (!categoryExists)
            {
                return BadRequest("Category not found");
            }
        }

        // Check for overlapping periods (excluding current record)
        var hasOverlap = await _context.WorkplaceLimits
            .Where(wl => wl.WorkplaceId == limit.WorkplaceId
                && wl.CategoryId == limit.CategoryId
                && wl.IsActive
                && wl.Id != limit.Id)
            .AnyAsync(wl =>
                (limit.PeriodFrom >= wl.PeriodFrom && limit.PeriodFrom <= wl.PeriodTo) ||
                (limit.PeriodTo >= wl.PeriodFrom && limit.PeriodTo <= wl.PeriodTo) ||
                (limit.PeriodFrom <= wl.PeriodFrom && limit.PeriodTo >= wl.PeriodTo));

        if (hasOverlap)
        {
            return BadRequest("A limit with overlapping period already exists for this workplace and category");
        }

        existingLimit.CategoryId = limit.CategoryId;
        existingLimit.PeriodFrom = limit.PeriodFrom;
        existingLimit.PeriodTo = limit.PeriodTo;
        existingLimit.LimitAmount = limit.LimitAmount;
        existingLimit.Currency = limit.Currency;
        existingLimit.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.WorkplaceLimits.AnyAsync(e => e.Id == id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLimit(Guid id)
    {
        var limit = await _context.WorkplaceLimits.FindAsync(id);
        if (limit == null)
        {
            return NotFound();
        }

        // Soft delete
        limit.IsActive = false;
        limit.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
