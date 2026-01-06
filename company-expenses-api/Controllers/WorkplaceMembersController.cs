using CompanyExpenses.Api.Data;
using CompanyExpenses.Api.DTOs;
using CompanyExpenses.Database.Data;
using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;
using CompanyExpenses.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CompanyExpenses.Api.Controllers;

/// <summary>
/// Controller for workplace member management - refactored to use Service layer
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WorkplaceMembersController : ControllerBase
{
    private readonly IWorkplaceMemberService _memberService;
    private readonly AuthDbContext _authDb;
    private readonly AppDbContext _appDb;
    private readonly ILogger<WorkplaceMembersController> _logger;

    public WorkplaceMembersController(
        IWorkplaceMemberService memberService,
        AuthDbContext authDb,
        AppDbContext appDb,
        ILogger<WorkplaceMembersController> logger)
    {
        _memberService = memberService;
        _authDb = authDb;
        _appDb = appDb;
        _logger = logger;
    }

    /// <summary>
    /// Get users with stats
    /// </summary>
    [HttpGet("users-with-stats")]
    public async Task<ActionResult> GetUsersWithStats([FromQuery] bool includeInactive = false)
    {
        try
        {
            var users = await _authDb.NetUsers
                .Where(u => includeInactive || u.IsActive)
                .ToListAsync();

            var userRoles = await _authDb.UserRoles.ToListAsync();
            var roles = await _authDb.Roles.ToListAsync();

            var userIds = users.Select(u => u.Id).ToList();

            var memberships = await _appDb.WorkplaceMembers
                .Include(m => m.Workplace)
                .Where(m => userIds.Contains(m.UserId))
                .ToListAsync();

            var expenses = await _appDb.Expenses
                .Where(e => userIds.Contains(e.EmployeeUserId))
                .GroupBy(e => e.EmployeeUserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Count = g.Count(),
                    Total = g.Sum(e => e.Amount)
                })
                .ToListAsync();

            var result = users.Select(u =>
            {
                var userRoleIds = userRoles.Where(ur => ur.UserId == u.Id).Select(ur => ur.RoleId).ToList();
                var userRoleNames = roles.Where(r => userRoleIds.Contains(r.Id)).Select(r => r.Name).ToList();
                var role = userRoleNames.FirstOrDefault() ?? "employee";

                var membership = memberships.FirstOrDefault(m => m.UserId == u.Id);
                var expenseStats = expenses.FirstOrDefault(e => e.UserId == u.Id);

                return new UserWithStatsDto
                {
                    Id = u.Id,
                    Name = u.UserName ?? u.Email ?? "Unknown",
                    Email = u.Email ?? "",
                    Role = role.ToLower(),
                    Workplace = membership?.Workplace?.Name ?? "Nepřiřazeno",
                    WorkplaceId = membership?.WorkplaceId,
                    IsActive = u.IsActive,
                    Status = u.IsActive ? "active" : "inactive",
                    ExpenseCount = expenseStats?.Count ?? 0,
                    TotalExpenses = expenseStats?.Total ?? 0
                };
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users with stats");
            return StatusCode(500, new { message = "Error loading users" });
        }
    }

    /// <summary>
    /// Get inactive users
    /// </summary>
    [HttpGet("users-with-stats/inactive")]
    public async Task<ActionResult> GetInactiveUsers()
    {
        try
        {
            var users = await _authDb.NetUsers
                .Where(u => !u.IsActive)
                .ToListAsync();

            var userRoles = await _authDb.UserRoles.ToListAsync();
            var roles = await _authDb.Roles.ToListAsync();

            var userIds = users.Select(u => u.Id).ToList();

            var memberships = await _appDb.WorkplaceMembers
                .Include(m => m.Workplace)
                .Where(m => userIds.Contains(m.UserId))
                .ToListAsync();

            var expenses = await _appDb.Expenses
                .Where(e => userIds.Contains(e.EmployeeUserId))
                .GroupBy(e => e.EmployeeUserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Count = g.Count(),
                    Total = g.Sum(e => e.Amount)
                })
                .ToListAsync();

            var result = users.Select(u =>
            {
                var userRoleIds = userRoles.Where(ur => ur.UserId == u.Id).Select(ur => ur.RoleId).ToList();
                var userRoleNames = roles.Where(r => userRoleIds.Contains(r.Id)).Select(r => r.Name).ToList();
                var role = userRoleNames.FirstOrDefault() ?? "employee";

                var membership = memberships.FirstOrDefault(m => m.UserId == u.Id);
                var expenseStats = expenses.FirstOrDefault(e => e.UserId == u.Id);

                return new UserWithStatsDto
                {
                    Id = u.Id,
                    Name = u.UserName ?? u.Email ?? "Unknown",
                    Email = u.Email ?? "",
                    Role = role.ToLower(),
                    Workplace = membership?.Workplace?.Name ?? "Nepřiřazeno",
                    WorkplaceId = membership?.WorkplaceId,
                    IsActive = u.IsActive,
                    Status = "inactive",
                    ExpenseCount = expenseStats?.Count ?? 0,
                    TotalExpenses = expenseStats?.Total ?? 0
                };
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inactive users");
            return StatusCode(500, new { message = "Error loading inactive users" });
        }
    }

    /// <summary>
    /// Get user detail
    /// </summary>
    [HttpGet("user/{userId}/detail")]
    public async Task<ActionResult> GetUserDetail(string userId)
    {
        try
        {
            var user = await _authDb.NetUsers.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var userRoles = await _authDb.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
            var roles = await _authDb.Roles.ToListAsync();
            var userRoleNames = roles.Where(r => userRoles.Any(ur => ur.RoleId == r.Id)).Select(r => r.Name).ToList();
            var role = userRoleNames.FirstOrDefault() ?? "employee";

            var memberships = await _appDb.WorkplaceMembers
                .Include(m => m.Workplace)
                .Where(m => m.UserId == userId)
                .Select(m => new UserMembershipDto
                {
                    Id = m.Id,
                    WorkplaceId = m.WorkplaceId,
                    WorkplaceName = m.Workplace!.Name,
                    PositionName = m.PositionName,
                    IsManager = m.IsManager,
                    JoinedAt = m.CreatedAt
                })
                .ToListAsync();

            // Get all expenses for this user
            var allExpenses = await _appDb.Expenses
                .Include(e => e.Workplace)
                .Include(e => e.Category)
                .Where(e => e.EmployeeUserId == userId)
                .ToListAsync();

            var expenses = allExpenses
                .OrderByDescending(e => e.SubmittedAt)
                .Select(e => new UserExpenseDto
                {
                    Id = e.Id,
                    Amount = e.Amount,
                    Currency = e.Currency,
                    ExpenseDate = e.ExpenseDate,
                    Description = e.Description,
                    Status = e.Status.ToString(),
                    CategoryName = e.Category != null ? e.Category.Name : "N/A",
                    WorkplaceName = e.Workplace != null ? e.Workplace.Name : "N/A",
                    SubmittedAt = e.SubmittedAt
                })
                .ToList();

            // Calculate expense stats
            var expenseStats = new UserExpenseStatsDto
            {
                Total = allExpenses.Sum(e => e.Amount),
                Count = allExpenses.Count,
                ByStatus = allExpenses
                    .GroupBy(e => e.Status.ToString())
                    .Select(g => new ExpenseStatusStatDto
                    {
                        Status = g.Key,
                        Count = g.Count(),
                        Total = g.Sum(e => e.Amount)
                    })
                    .ToList()
            };

            // Get all approvals
            var allApprovals = await _appDb.ExpenseApprovals
                .Where(a => a.ActorUserId == userId)
                .ToListAsync();

            var approvals = allApprovals
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new UserApprovalDto
                {
                    Id = a.Id,
                    ExpenseId = a.ExpenseId,
                    Action = a.Action.ToString(),
                    Note = a.Note,
                    CreatedAt = a.CreatedAt
                })
                .ToList();

            // Calculate approval stats
            var approvalStats = new UserApprovalStatsDto
            {
                Count = allApprovals.Count,
                Approved = allApprovals.Count(a => a.Action == Models.Enums.ApprovalAction.Approved),
                Rejected = allApprovals.Count(a => a.Action == Models.Enums.ApprovalAction.Rejected)
            };

            // Get invitations
            var invitations = await _appDb.Invitations
                .Include(i => i.Workplace)
                .Where(i => i.Email == user.Email)
                .Select(i => new UserInvitationDto
                {
                    Id = i.Id,
                    Email = i.Email,
                    WorkplaceName = i.Workplace != null ? i.Workplace.Name : "N/A",
                    Status = i.Status.ToString(),
                    CreatedAt = i.CreatedAt,
                    AcceptedAt = i.AcceptedAt,
                    ExpiresAt = i.ExpiresAt
                })
                .ToListAsync();

            // Calculate invitation stats
            var invitationStats = new UserInvitationStatsDto
            {
                Count = invitations.Count,
                Pending = invitations.Count(i => i.Status == "Pending"),
                Accepted = invitations.Count(i => i.Status == "Accepted"),
                Expired = invitations.Count(i => i.Status == "Expired")
            };

            var result = new UserDetailDto
            {
                Id = user.Id,
                Name = user.UserName ?? user.Email ?? "Unknown",
                Email = user.Email ?? "",
                Role = role.ToLower(),
                IsActive = user.IsActive,
                CreatedAt = DateTime.UtcNow, // TODO: Get actual created date from user
                Memberships = memberships,
                Expenses = expenses,
                ExpenseStats = expenseStats,
                Approvals = approvals,
                ApprovalStats = approvalStats,
                Invitations = invitations,
                InvitationStats = invitationStats
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user detail for {UserId}", userId);
            return StatusCode(500, new { message = "Error loading user detail" });
        }
    }

    /// <summary>
    /// Deactivate user
    /// </summary>
    [HttpPatch("user/{userId}/deactivate")]
    public async Task<ActionResult> DeactivateUser(string userId)
    {
        try
        {
            var user = await _authDb.NetUsers.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            user.IsActive = false;
            await _authDb.SaveChangesAsync();

            return Ok(new { message = "User deactivated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user {UserId}", userId);
            return StatusCode(500, new { message = "Error deactivating user" });
        }
    }

    /// <summary>
    /// Reactivate user
    /// </summary>
    [HttpPatch("user/{userId}/reactivate")]
    public async Task<ActionResult> ReactivateUser(string userId)
    {
        try
        {
            var user = await _authDb.NetUsers.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            user.IsActive = true;
            await _authDb.SaveChangesAsync();

            return Ok(new { message = "User reactivated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating user {UserId}", userId);
            return StatusCode(500, new { message = "Error reactivating user" });
        }
    }

    /// <summary>
    /// Delete user
    /// </summary>
    [HttpDelete("user/{userId}")]
    public async Task<ActionResult> DeleteUser(string userId)
    {
        try
        {
            var user = await _authDb.NetUsers.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            // Check if user has expenses or is a member of workplaces
            var hasExpenses = await _appDb.Expenses.AnyAsync(e => e.EmployeeUserId == userId);
            if (hasExpenses)
                return BadRequest(new { message = "Cannot delete user with expenses" });

            // Remove from workplaces
            var memberships = await _appDb.WorkplaceMembers.Where(m => m.UserId == userId).ToListAsync();
            _appDb.WorkplaceMembers.RemoveRange(memberships);
            await _appDb.SaveChangesAsync();

            _authDb.NetUsers.Remove(user);
            await _authDb.SaveChangesAsync();

            return Ok(new { message = "User deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            return StatusCode(500, new { message = "Error deleting user" });
        }
    }

    /// <summary>
    /// Change user role
    /// </summary>
    [HttpPatch("user/{userId}/role")]
    public async Task<ActionResult> ChangeUserRole(string userId, [FromBody] ChangeRoleRequest request)
    {
        try
        {
            var user = await _authDb.NetUsers.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var role = await _authDb.Roles.FindAsync(request.RoleId);
            if (role == null)
                return NotFound(new { message = "Role not found" });

            // Remove existing roles
            var existingRoles = await _authDb.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
            _authDb.UserRoles.RemoveRange(existingRoles);

            // Add new role
            _authDb.UserRoles.Add(new Microsoft.AspNetCore.Identity.IdentityUserRole<string>
            {
                UserId = userId,
                RoleId = request.RoleId
            });

            await _authDb.SaveChangesAsync();

            return Ok(new { message = "Role changed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing role for user {UserId}", userId);
            return StatusCode(500, new { message = "Error changing user role" });
        }
    }

    /// <summary>
    /// Add user to workplace
    /// </summary>
    [HttpPost("user/{userId}/workplace")]
    public async Task<ActionResult> AddUserToWorkplace(string userId, [FromBody] AddToWorkplaceRequest request)
    {
        try
        {
            var user = await _authDb.NetUsers.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var workplace = await _appDb.Workplaces.FindAsync(request.WorkplaceId);
            if (workplace == null)
                return NotFound(new { message = "Workplace not found" });

            // Check if already a member
            var existing = await _appDb.WorkplaceMembers
                .FirstOrDefaultAsync(m => m.UserId == userId && m.WorkplaceId == request.WorkplaceId);
            if (existing != null)
                return BadRequest(new { message = "User is already a member of this workplace" });

            var member = new Models.Entities.WorkplaceMember
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                WorkplaceId = request.WorkplaceId,
                PositionName = request.PositionName,
                IsManager = request.IsManager,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = GetCurrentUserId() ?? "system"
            };

            _appDb.WorkplaceMembers.Add(member);
            await _appDb.SaveChangesAsync();

            return Ok(new { message = "User added to workplace successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user {UserId} to workplace", userId);
            return StatusCode(500, new { message = "Error adding user to workplace" });
        }
    }

    /// <summary>
    /// Get all members of a workplace
    /// </summary>
    [HttpGet("workplace/{workplaceId}")]
    public async Task<ActionResult> GetWorkplaceMembers(Guid workplaceId)
    {
        var result = await _memberService.GetMembersByWorkplaceAsync(workplaceId);
        return HandleResult(result);
    }

    /// <summary>
    /// Get member by ID
    /// </summary>
    [HttpGet("{workplaceId}/{id}")]
    public async Task<ActionResult> GetMember(Guid workplaceId, Guid id)
    {
        var result = await _memberService.GetMemberByIdAsync(workplaceId, id);
        return HandleResult(result);
    }

    /// <summary>
    /// Add a member to workplace
    /// </summary>
    [HttpPost("{workplaceId}")]
    public async Task<ActionResult> AddMember(Guid workplaceId, [FromBody] CreateWorkplaceMemberDto dto)
    {
        var userId = GetCurrentUserId() ?? "system";
        var result = await _memberService.AddMemberAsync(workplaceId, dto, userId);

        if (result.IsSuccess && result.Data != null)
        {
            return CreatedAtAction(nameof(GetMember),
                new { workplaceId = workplaceId, id = result.Data.Id },
                result.Data);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Update member
    /// </summary>
    [HttpPut("{workplaceId}/{id}")]
    public async Task<IActionResult> UpdateMember(Guid workplaceId, Guid id, [FromBody] UpdateWorkplaceMemberDto dto)
    {
        var result = await _memberService.UpdateMemberAsync(workplaceId, id, dto);
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return HandleResult(result);
    }

    /// <summary>
    /// Remove member from workplace
    /// </summary>
    [HttpDelete("{workplaceId}/{id}")]
    public async Task<IActionResult> RemoveMember(Guid workplaceId, Guid id)
    {
        var result = await _memberService.RemoveMemberAsync(workplaceId, id);
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return HandleResult(result);
    }

    /// <summary>
    /// Get workplaces for a user
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult> GetUserWorkplaces(string userId)
    {
        var result = await _memberService.GetWorkplacesByUserAsync(userId);
        return HandleResult(result);
    }

    /// <summary>
    /// Check if user is manager of workplace
    /// </summary>
    [HttpGet("is-manager/{workplaceId}/{userId}")]
    public async Task<ActionResult> IsUserManager(Guid workplaceId, string userId)
    {
        var result = await _memberService.IsUserManagerAsync(userId, workplaceId);
        return HandleResult(result);
    }

    #region Helper Methods

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private ActionResult HandleResult<T>(ServiceResult<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(new { message = result.ErrorMessage }),
            ServiceErrorType.BadRequest => BadRequest(new { message = result.ErrorMessage }),
            ServiceErrorType.Unauthorized => Unauthorized(new { message = result.ErrorMessage }),
            _ => StatusCode(500, new { message = result.ErrorMessage })
        };
    }

    private ActionResult HandleResult(ServiceResult result)
    {
        if (result.IsSuccess)
        {
            return Ok();
        }

        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(new { message = result.ErrorMessage }),
            ServiceErrorType.BadRequest => BadRequest(new { message = result.ErrorMessage }),
            ServiceErrorType.Unauthorized => Unauthorized(new { message = result.ErrorMessage }),
            _ => StatusCode(500, new { message = result.ErrorMessage })
        };
    }

    #endregion
}
