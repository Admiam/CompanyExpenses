using CompanyExpenses.Database.Data;
using CompanyExpenses.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CompanyExpenses.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkplaceMembersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<WorkplaceMembersController> _logger;

    public WorkplaceMembersController(AppDbContext context, ILogger<WorkplaceMembersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all members of all workplaces
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkplaceMember>>> GetAllMembers()
    {
        return await _context.WorkplaceMembers
            .Include(m => m.Workplace)
            .ToListAsync();
    }

    /// <summary>
    /// Gets members of a specific workplace
    /// </summary>
    [HttpGet("workplace/{workplaceId}")]
    public async Task<ActionResult<IEnumerable<WorkplaceMember>>> GetWorkplaceMembers(Guid workplaceId)
    {
        var workplace = await _context.Workplaces.FindAsync(workplaceId);
        if (workplace == null)
        {
            return NotFound(new { message = "Workplace not found" });
        }

        var members = await _context.WorkplaceMembers
            .Where(m => m.WorkplaceId == workplaceId)
            .Include(m => m.Workplace)
            .ToListAsync();

        return members;
    }

    /// <summary>
    /// Gets workplaces where the user is a member
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<WorkplaceMember>>> GetUserWorkplaces(string userId)
    {
        var memberships = await _context.WorkplaceMembers
            .Where(m => m.UserId == userId)
            .Include(m => m.Workplace)
            .ToListAsync();

        return memberships;
    }

    /// <summary>
    /// Gets a specific member by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<WorkplaceMember>> GetMember(Guid id)
    {
        var member = await _context.WorkplaceMembers
            .Include(m => m.Workplace)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (member == null)
        {
            return NotFound();
        }

        return member;
    }

    /// <summary>
    /// Adds a member to a workplace
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<WorkplaceMember>> AddMember(CreateWorkplaceMemberDto dto)
    {
        // Check if workplace exists
        var workplace = await _context.Workplaces.FindAsync(dto.WorkplaceId);
        if (workplace == null)
        {
            return BadRequest(new { message = "Workplace not found" });
        }

        // Check if already a member
        var existingMember = await _context.WorkplaceMembers
            .FirstOrDefaultAsync(m => m.WorkplaceId == dto.WorkplaceId && m.UserId == dto.UserId);

        if (existingMember != null)
        {
            return BadRequest(new { message = "User is already a member of this workplace" });
        }

        var member = new WorkplaceMember
        {
            Id = Guid.NewGuid(),
            WorkplaceId = dto.WorkplaceId,
            UserId = dto.UserId,
            PositionName = dto.PositionName,
            IsManager = dto.IsManager,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-user" // TODO: Získat z authentication
        };

        _context.WorkplaceMembers.Add(member);
        await _context.SaveChangesAsync();

        // Load member with navigation properties
        member = await _context.WorkplaceMembers
            .Include(m => m.Workplace)
            .FirstAsync(m => m.Id == member.Id);

        return CreatedAtAction(nameof(GetMember), new { id = member.Id }, member);
    }

    /// <summary>
    /// Updates a member (e.g. position, manager status)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMember(Guid id, UpdateWorkplaceMemberDto dto)
    {
        var member = await _context.WorkplaceMembers.FindAsync(id);

        if (member == null)
        {
            return NotFound();
        }

        member.PositionName = dto.PositionName;
        member.IsManager = dto.IsManager;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.WorkplaceMembers.AnyAsync(m => m.Id == id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Removes a member from a workplace
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveMember(Guid id)
    {
        var member = await _context.WorkplaceMembers.FindAsync(id);

        if (member == null)
        {
            return NotFound();
        }

        _context.WorkplaceMembers.Remove(member);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Sets/removes user as workplace manager
    /// </summary>
    [HttpPatch("{id}/manager")]
    public async Task<IActionResult> ToggleManager(Guid id, [FromBody] bool isManager)
    {
        var member = await _context.WorkplaceMembers.FindAsync(id);

        if (member == null)
        {
            return NotFound();
        }

        member.IsManager = isManager;
        await _context.SaveChangesAsync();

        return Ok(new { message = isManager ? "User has been appointed as manager" : "Manager role has been removed from user" });
    }
}

// DTOs
public class CreateWorkplaceMemberDto
{
    public Guid WorkplaceId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? PositionName { get; set; }
    public bool IsManager { get; set; } = false;
}

public class UpdateWorkplaceMemberDto
{
    public string? PositionName { get; set; }
    public bool IsManager { get; set; }
}
