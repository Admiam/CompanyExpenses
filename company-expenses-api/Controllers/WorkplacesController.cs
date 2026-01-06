using CompanyExpenses.Database.Data;
using CompanyExpenses.Models.Entities;
using CompanyExpenses.Api.DTOs;
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
    public async Task<ActionResult<IEnumerable<WorkplaceDto>>> GetWorkplaces()
    {
        var workplaces = await _context.Workplaces
            .Include(w => w.Members)
            .ToListAsync();

        var workplaceDtos = workplaces.Select(w => new WorkplaceDto
        {
            Id = w.Id,
            Name = w.Name,
            Code = w.Code,
            IsActive = w.IsActive,
            CreatedAt = w.CreatedAt,
            CreatedBy = w.CreatedBy,
            Members = w.Members.Select(m => new WorkplaceMemberDto
            {
                Id = m.Id,
                WorkplaceId = m.WorkplaceId,
                UserId = m.UserId,
                PositionName = m.PositionName,
                IsManager = m.IsManager,
                CreatedAt = m.CreatedAt,
                CreatedBy = m.CreatedBy
            }).ToList()
        }).ToList();

        return Ok(workplaceDtos);
    }

    /// <summary>
    /// Získá konkrétní pracoviště podle ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<WorkplaceDetailDto>> GetWorkplace(Guid id)
    {
        var workplace = await _context.Workplaces
            .Include(w => w.Members)
            .Include(w => w.Limits)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workplace == null)
        {
            return NotFound();
        }

        var workplaceDto = new WorkplaceDetailDto
        {
            Id = workplace.Id,
            Name = workplace.Name,
            Code = workplace.Code,
            IsActive = workplace.IsActive,
            CreatedAt = workplace.CreatedAt,
            CreatedBy = workplace.CreatedBy,
            Members = workplace.Members.Select(m => new WorkplaceMemberDto
            {
                Id = m.Id,
                WorkplaceId = m.WorkplaceId,
                UserId = m.UserId,
                PositionName = m.PositionName,
                IsManager = m.IsManager,
                CreatedAt = m.CreatedAt,
                CreatedBy = m.CreatedBy
            }).ToList(),
            Limits = workplace.Limits.Select(l => new WorkplaceLimitDto
            {
                Id = l.Id,
                WorkplaceId = l.WorkplaceId,
                PeriodFrom = l.PeriodFrom,
                PeriodTo = l.PeriodTo,
                LimitAmount = l.LimitAmount,
                Currency = l.Currency,
                CategoryId = l.CategoryId,
                IsActive = l.IsActive,
                CreatedAt = l.CreatedAt,
                CreatedBy = l.CreatedBy
            }).ToList()
        };

        return Ok(workplaceDto);
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
    /// Získá informace o závislostech pracoviště
    /// </summary>
    [HttpGet("{id}/dependencies")]
    public async Task<ActionResult<WorkplaceDependenciesDto>> GetWorkplaceDependencies(Guid id)
    {
        var workplace = await _context.Workplaces.FindAsync(id);
        if (workplace == null)
        {
            return NotFound();
        }

        var membersCount = await _context.WorkplaceMembers.CountAsync(m => m.WorkplaceId == id);
        var limitsCount = await _context.WorkplaceLimits.CountAsync(l => l.WorkplaceId == id);
        var invitationsCount = await _context.Invitations.CountAsync(i => i.WorkplaceId == id);
        var expensesCount = await _context.Expenses.CountAsync(e => e.WorkplaceId == id);

        return Ok(new WorkplaceDependenciesDto
        {
            WorkplaceId = id,
            MembersCount = membersCount,
            LimitsCount = limitsCount,
            InvitationsCount = invitationsCount,
            ExpensesCount = expensesCount,
            CanDelete = membersCount == 0 && limitsCount == 0 && invitationsCount == 0 && expensesCount == 0
        });
    }

    /// <summary>
    /// Smaže pracoviště z databáze (pouze pokud nemá závislosti)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkplace(Guid id)
    {
        var workplace = await _context.Workplaces.FindAsync(id);
        if (workplace == null)
        {
            return NotFound();
        }

        // Check dependencies
        var membersCount = await _context.WorkplaceMembers.CountAsync(m => m.WorkplaceId == id);
        var limitsCount = await _context.WorkplaceLimits.CountAsync(l => l.WorkplaceId == id);
        var invitationsCount = await _context.Invitations.CountAsync(i => i.WorkplaceId == id);
        var expensesCount = await _context.Expenses.CountAsync(e => e.WorkplaceId == id);

        if (membersCount > 0 || limitsCount > 0 || invitationsCount > 0 || expensesCount > 0)
        {
            return BadRequest(new
            {
                message = "Cannot delete workplace with existing dependencies",
                dependencies = new
                {
                    membersCount,
                    limitsCount,
                    invitationsCount,
                    expensesCount
                }
            });
        }

        _context.Workplaces.Remove(workplace);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> WorkplaceExists(Guid id)
    {
        return await _context.Workplaces.AnyAsync(e => e.Id == id);
    }
}

public class WorkplaceDependenciesDto
{
    public Guid WorkplaceId { get; set; }
    public int MembersCount { get; set; }
    public int LimitsCount { get; set; }
    public int InvitationsCount { get; set; }
    public int ExpensesCount { get; set; }
    public bool CanDelete { get; set; }
}
