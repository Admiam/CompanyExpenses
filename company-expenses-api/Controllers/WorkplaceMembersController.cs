using CompanyExpenses.Database.Data;
using CompanyExpenses.Models.Entities;
using CompanyExpenses.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CompanyExpenses.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkplaceMembersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly AuthDbContext _authContext;
    private readonly ILogger<WorkplaceMembersController> _logger;

    public WorkplaceMembersController(
        AppDbContext context,
        AuthDbContext authContext,
        ILogger<WorkplaceMembersController> logger)
    {
        _context = context;
        _authContext = authContext;
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
    /// Gets all users with their expense statistics
    /// </summary>
    [HttpGet("users-with-stats")]
    public async Task<ActionResult> GetUsersWithStats()
    {
        // Get ALL users from auth database
        var users = await _authContext.NetUsers.ToListAsync();

        // Get all user IDs
        var userIds = users.Select(u => u.Id).ToList();

        // Get all members with their workplace info
        var members = await _context.WorkplaceMembers
            .Include(m => m.Workplace)
            .Where(m => userIds.Contains(m.UserId))
            .ToListAsync();

        // Get user roles
        var userRoles = await _authContext.UserRoles.ToListAsync();
        var roles = await _authContext.Roles.ToListAsync();

        // Get expense statistics for all users
        var expenseStats = await _context.Expenses
            .Where(e => userIds.Contains(e.EmployeeUserId))
            .GroupBy(e => e.EmployeeUserId)
            .Select(g => new
            {
                UserId = g.Key,
                ExpenseCount = g.Count(),
                TotalExpenses = g.Sum(e => e.Amount)
            })
            .ToListAsync();

        // Combine all data
        var result = users.Select(user =>
        {
            var member = members.FirstOrDefault(m => m.UserId == user.Id);
            var stats = expenseStats.FirstOrDefault(s => s.UserId == user.Id);
            var userRole = userRoles.FirstOrDefault(ur => ur.UserId == user.Id);
            var role = userRole != null ? roles.FirstOrDefault(r => r.Id == userRole.RoleId) : null;

            return new
            {
                id = user.Id,
                name = user.UserName,
                email = user.Email,
                role = role?.Name?.ToLower() ?? "employee",
                workplace = member?.Workplace?.Name ?? "N/A",
                workplaceId = member?.WorkplaceId,
                status = "active",
                expenseCount = stats?.ExpenseCount ?? 0,
                totalExpenses = stats?.TotalExpenses ?? 0
            };
        }).ToList();

        return Ok(result);
    }

    /// <summary>
    /// Gets detailed information about a specific user
    /// </summary>
    [HttpGet("user/{userId}/detail")]
    public async Task<ActionResult> GetUserDetail(string userId)
    {
        // Get user from auth database
        var user = await _authContext.NetUsers.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        // Get user role
        var userRole = await _authContext.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId);
        var role = userRole != null ? await _authContext.Roles.FirstOrDefaultAsync(r => r.Id == userRole.RoleId) : null;

        // Get user's workplace memberships
        var memberships = await _context.WorkplaceMembers
            .Include(m => m.Workplace)
            .Where(m => m.UserId == userId)
            .Select(m => new
            {
                id = m.Id,
                workplaceId = m.Workplace!.Id,
                workplaceName = m.Workplace.Name,
                positionName = m.PositionName,
                isManager = m.IsManager,
                joinedAt = m.CreatedAt
            })
            .ToListAsync();

        // Get user's expenses
        var expenses = await _context.Expenses
            .Include(e => e.Category)
            .Include(e => e.Workplace)
            .Where(e => e.EmployeeUserId == userId)
            .OrderByDescending(e => e.ExpenseDate)
            .Select(e => new
            {
                id = e.Id,
                amount = e.Amount,
                currency = e.Currency,
                expenseDate = e.ExpenseDate,
                description = e.Description,
                status = e.Status.ToString(),
                categoryName = e.Category!.Name,
                workplaceName = e.Workplace!.Name,
                submittedAt = e.SubmittedAt
            })
            .ToListAsync();

        // Get user's expense approvals (actions they took)
        var approvals = await _context.ExpenseApprovals
            .Include(a => a.Expense)
                .ThenInclude(e => e!.Category)
            .Where(a => a.ActorUserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new
            {
                id = a.Id,
                expenseId = a.ExpenseId,
                action = a.Action.ToString(),
                note = a.Note,
                createdAt = a.CreatedAt,
                expenseAmount = a.Expense!.Amount,
                expenseCurrency = a.Expense.Currency,
                expenseDescription = a.Expense.Description,
                categoryName = a.Expense.Category!.Name
            })
            .ToListAsync();

        // Get invitations sent by this user
        var invitations = await _context.Invitations
            .Include(i => i.Workplace)
            .Where(i => i.InvitedByUserId == userId)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new
            {
                id = i.Id,
                email = i.Email,
                workplaceName = i.Workplace != null ? i.Workplace.Name : null,
                status = i.Status.ToString(),
                createdAt = i.CreatedAt,
                expiresAt = i.ExpiresAt,
                acceptedAt = i.AcceptedAt
            })
            .ToListAsync();

        // Get expense statistics
        var expenseStats = await _context.Expenses
            .Where(e => e.EmployeeUserId == userId)
            .GroupBy(e => e.Status)
            .Select(g => new
            {
                status = g.Key.ToString(),
                count = g.Count(),
                total = g.Sum(e => e.Amount)
            })
            .ToListAsync();

        var result = new
        {
            // Basic user info
            id = user.Id,
            name = user.UserName,
            email = user.Email,
            role = role?.Name?.ToLower() ?? "employee",
            createdAt = DateTime.UtcNow, // IdentityUser doesn't have CreatedAt, using current time as fallback

            // Memberships
            memberships = memberships,

            // Expenses
            expenses = expenses,
            expenseStats = new
            {
                total = expenses.Sum(e => e.amount),
                count = expenses.Count,
                byStatus = expenseStats
            },

            // Approvals
            approvals = approvals,
            approvalStats = new
            {
                count = approvals.Count,
                approved = approvals.Count(a => a.action == "Approve"),
                rejected = approvals.Count(a => a.action == "Reject")
            },

            // Invitations
            invitations = invitations,
            invitationStats = new
            {
                count = invitations.Count,
                pending = invitations.Count(i => i.status == "Pending"),
                accepted = invitations.Count(i => i.status == "Accepted"),
                expired = invitations.Count(i => i.status == "Expired")
            }
        };

        return Ok(result);
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

    /// <summary>
    /// Deletes a user and all their related data from the system
    /// This includes: WorkplaceMembers, Expenses, ExpenseApprovals, Invitations, and Identity user
    /// </summary>
    [HttpDelete("user/{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Check if user exists
            var user = await _authContext.NetUsers.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // 1. Delete workplace memberships
            var memberships = await _context.WorkplaceMembers
                .Where(m => m.UserId == userId)
                .ToListAsync();
            _context.WorkplaceMembers.RemoveRange(memberships);

            // 2. Delete or anonymize expenses (we'll soft delete by marking them)
            var expenses = await _context.Expenses
                .Where(e => e.EmployeeUserId == userId)
                .ToListAsync();

            foreach (var expense in expenses)
            {
                expense.IsDeleted = true;
            }

            // 3. Delete expense approvals
            var approvals = await _context.ExpenseApprovals
                .Where(a => a.ActorUserId == userId)
                .ToListAsync();
            _context.ExpenseApprovals.RemoveRange(approvals);

            // 4. Update invitations (mark as cancelled or remove)
            // Cancel invitations created by the user
            var invitationsCreatedByUser = await _context.Invitations
                .Where(i => i.InvitedByUserId == userId)
                .ToListAsync();

            foreach (var invitation in invitationsCreatedByUser)
            {
                invitation.Status = Models.Enums.InvitationStatus.Cancelled;
            }

            // Also need to get user's email to cancel invitations sent TO this user
            var invitationsSentToUser = await _context.Invitations
                .Where(i => i.InviteeEmail == user.Email)
                .ToListAsync();

            foreach (var invitation in invitationsSentToUser)
            {
                invitation.Status = Models.Enums.InvitationStatus.Cancelled;
            }

            // Save changes to application database
            await _context.SaveChangesAsync();

            // 5. Delete user from Identity system directly through AuthDbContext
            var identityUser = await _authContext.NetUsers.FirstOrDefaultAsync(u => u.Id == userId);
            if (identityUser != null)
            {
                // Remove user roles
                var userRoles = await _authContext.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
                _authContext.UserRoles.RemoveRange(userRoles);

                // Remove the user
                _authContext.NetUsers.Remove(identityUser);
                await _authContext.SaveChangesAsync();
            }

            // Commit transaction
            await transaction.CommitAsync();

            _logger.LogInformation("User {UserId} and all related data successfully deleted", userId);
            return Ok(new { message = "User successfully deleted" });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while deleting the user" });
        }
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
