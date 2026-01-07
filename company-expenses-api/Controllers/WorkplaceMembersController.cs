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
/// Controller for workplace member and user management operations.
/// Handles user statistics, activation/deactivation, role changes, and workplace assignments.
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
    /// Retrieves all users with their statistics including expense counts and totals.
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive users in the result.</param>
    /// <returns>A list of users with their statistics.</returns>
    [HttpGet("users-with-stats")]
    public async Task<ActionResult> GetUsersWithStats([FromQuery] bool includeInactive = false)
    {
        try
        {
            _logger.LogInformation("Fetching users with stats, includeInactive: {IncludeInactive}", includeInactive);
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
    /// Retrieves only inactive users with their statistics.
    /// </summary>
    /// <returns>A list of inactive users with their statistics.</returns>
    [HttpGet("users-with-stats/inactive")]
    public async Task<ActionResult> GetInactiveUsers()
    {
        try
        {
            _logger.LogInformation("Fetching inactive users with stats");
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
    /// Retrieves detailed information about a specific user including memberships, expenses, and approvals.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>Detailed user information or NotFound if user doesn't exist.</returns>
    [HttpGet("user/{userId}/detail")]
    public async Task<ActionResult> GetUserDetail(string userId)
    {
        try
        {
            _logger.LogInformation("Fetching detail for user {UserId}", userId);
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
    /// Deactivates a user account, preventing them from logging in.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to deactivate.</param>
    /// <returns>Success message or error response.</returns>
    [HttpPatch("user/{userId}/deactivate")]
    public async Task<ActionResult> DeactivateUser(string userId)
    {
        try
        {
            _logger.LogInformation("Deactivating user {UserId}", userId);
            var user = await _authDb.NetUsers.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found for deactivation: {UserId}", userId);
                return NotFound(new { message = "User not found" });
            }

            user.IsActive = false;
            await _authDb.SaveChangesAsync();

            _logger.LogInformation("User {UserId} deactivated successfully", userId);
            return Ok(new { message = "User deactivated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user {UserId}", userId);
            return StatusCode(500, new { message = "Error deactivating user" });
        }
    }

    /// <summary>
    /// Reactivates a previously deactivated user account.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to reactivate.</param>
    /// <returns>Success message or error response.</returns>
    [HttpPatch("user/{userId}/reactivate")]
    public async Task<ActionResult> ReactivateUser(string userId)
    {
        try
        {
            _logger.LogInformation("Reactivating user {UserId}", userId);
            var user = await _authDb.NetUsers.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found for reactivation: {UserId}", userId);
                return NotFound(new { message = "User not found" });
            }

            user.IsActive = true;
            await _authDb.SaveChangesAsync();

            _logger.LogInformation("User {UserId} reactivated successfully", userId);
            return Ok(new { message = "User reactivated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating user {UserId}", userId);
            return StatusCode(500, new { message = "Error reactivating user" });
        }
    }

    /// <summary>
    /// Permanently deletes a user. Only possible if the user has no expenses.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to delete.</param>
    /// <returns>Success message or error response if user has dependencies.</returns>
    [HttpDelete("user/{userId}")]
    public async Task<ActionResult> DeleteUser(string userId)
    {
        try
        {
            _logger.LogInformation("Attempting to delete user {UserId}", userId);
            var user = await _authDb.NetUsers.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found for deletion: {UserId}", userId);
                return NotFound(new { message = "User not found" });
            }

            var hasExpenses = await _appDb.Expenses.AnyAsync(e => e.EmployeeUserId == userId);
            if (hasExpenses)
            {
                _logger.LogWarning("Cannot delete user {UserId} - user has expenses", userId);
                return BadRequest(new { message = "Cannot delete user with expenses" });
            }

            var memberships = await _appDb.WorkplaceMembers.Where(m => m.UserId == userId).ToListAsync();
            _appDb.WorkplaceMembers.RemoveRange(memberships);
            await _appDb.SaveChangesAsync();

            _authDb.NetUsers.Remove(user);
            await _authDb.SaveChangesAsync();

            _logger.LogInformation("User {UserId} deleted successfully", userId);
            return Ok(new { message = "User deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            return StatusCode(500, new { message = "Error deleting user" });
        }
    }

    /// <summary>
    /// Changes the role assigned to a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="request">The request containing the new role ID.</param>
    /// <returns>Success message or error response.</returns>
    [HttpPatch("user/{userId}/role")]
    public async Task<ActionResult> ChangeUserRole(string userId, [FromBody] ChangeRoleRequest request)
    {
        try
        {
            _logger.LogInformation("Changing role for user {UserId} to role {RoleId}", userId, request.RoleId);
            var user = await _authDb.NetUsers.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found for role change: {UserId}", userId);
                return NotFound(new { message = "User not found" });
            }

            var role = await _authDb.Roles.FindAsync(request.RoleId);
            if (role == null)
            {
                _logger.LogWarning("Role not found: {RoleId}", request.RoleId);
                return NotFound(new { message = "Role not found" });
            }

            var existingRoles = await _authDb.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
            _authDb.UserRoles.RemoveRange(existingRoles);

            _authDb.UserRoles.Add(new Microsoft.AspNetCore.Identity.IdentityUserRole<string>
            {
                UserId = userId,
                RoleId = request.RoleId
            });

            await _authDb.SaveChangesAsync();

            _logger.LogInformation("Role changed successfully for user {UserId}", userId);
            return Ok(new { message = "Role changed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing role for user {UserId}", userId);
            return StatusCode(500, new { message = "Error changing user role" });
        }
    }

    /// <summary>
    /// Adds a user to a workplace as a member.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="request">The request containing workplace details and position.</param>
    /// <returns>Success message or error response.</returns>
    [HttpPost("user/{userId}/workplace")]
    public async Task<ActionResult> AddUserToWorkplace(string userId, [FromBody] AddToWorkplaceRequest request)
    {
        try
        {
            _logger.LogInformation("Adding user {UserId} to workplace {WorkplaceId}", userId, request.WorkplaceId);
            var user = await _authDb.NetUsers.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return NotFound(new { message = "User not found" });
            }

            var workplace = await _appDb.Workplaces.FindAsync(request.WorkplaceId);
            if (workplace == null)
            {
                _logger.LogWarning("Workplace not found: {WorkplaceId}", request.WorkplaceId);
                return NotFound(new { message = "Workplace not found" });
            }

            var existing = await _appDb.WorkplaceMembers
                .FirstOrDefaultAsync(m => m.UserId == userId && m.WorkplaceId == request.WorkplaceId);
            if (existing != null)
            {
                _logger.LogWarning("User {UserId} is already a member of workplace {WorkplaceId}", userId, request.WorkplaceId);
                return BadRequest(new { message = "User is already a member of this workplace" });
            }

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

            _logger.LogInformation("User {UserId} added to workplace {WorkplaceId} successfully", userId, request.WorkplaceId);
            return Ok(new { message = "User added to workplace successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user {UserId} to workplace", userId);
            return StatusCode(500, new { message = "Error adding user to workplace" });
        }
    }

    /// <summary>
    /// Retrieves all members of a specific workplace.
    /// </summary>
    /// <param name="workplaceId">The unique identifier of the workplace.</param>
    /// <returns>A list of workplace members.</returns>
    [HttpGet("workplace/{workplaceId}")]
    public async Task<ActionResult> GetWorkplaceMembers(Guid workplaceId)
    {
        _logger.LogInformation("Fetching members for workplace {WorkplaceId}", workplaceId);
        var result = await _memberService.GetMembersByWorkplaceAsync(workplaceId);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves a specific member by ID within a workplace.
    /// </summary>
    /// <param name="workplaceId">The unique identifier of the workplace.</param>
    /// <param name="id">The unique identifier of the member.</param>
    /// <returns>The member details if found, otherwise NotFound.</returns>
    [HttpGet("{workplaceId}/{id}")]
    public async Task<ActionResult> GetMember(Guid workplaceId, Guid id)
    {
        _logger.LogInformation("Fetching member {MemberId} from workplace {WorkplaceId}", id, workplaceId);
        var result = await _memberService.GetMemberByIdAsync(workplaceId, id);
        return HandleResult(result);
    }

    /// <summary>
    /// Adds a new member to a workplace.
    /// </summary>
    /// <param name="workplaceId">The unique identifier of the workplace.</param>
    /// <param name="dto">The member creation data transfer object.</param>
    /// <returns>The created member with its ID, or an error response.</returns>
    [HttpPost("{workplaceId}")]
    public async Task<ActionResult> AddMember(Guid workplaceId, [FromBody] CreateWorkplaceMemberDto dto)
    {
        var userId = GetCurrentUserId() ?? "system";
        _logger.LogInformation("Adding member to workplace {WorkplaceId} by user {UserId}", workplaceId, userId);

        var result = await _memberService.AddMemberAsync(workplaceId, dto, userId);

        if (result.IsSuccess && result.Data != null)
        {
            _logger.LogInformation("Member added successfully with ID: {MemberId}", result.Data.Id);
            return CreatedAtAction(nameof(GetMember),
                new { workplaceId = workplaceId, id = result.Data.Id },
                result.Data);
        }

        _logger.LogWarning("Failed to add member: {ErrorMessage}", result.ErrorMessage);
        return HandleResult(result);
    }

    /// <summary>
    /// Updates an existing workplace member's information.
    /// </summary>
    /// <param name="workplaceId">The unique identifier of the workplace.</param>
    /// <param name="id">The unique identifier of the member to update.</param>
    /// <param name="dto">The member update data transfer object.</param>
    /// <returns>NoContent on success, or error response.</returns>
    [HttpPut("{workplaceId}/{id}")]
    public async Task<IActionResult> UpdateMember(Guid workplaceId, Guid id, [FromBody] UpdateWorkplaceMemberDto dto)
    {
        _logger.LogInformation("Updating member {MemberId} in workplace {WorkplaceId}", id, workplaceId);
        var result = await _memberService.UpdateMemberAsync(workplaceId, id, dto);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Member {MemberId} updated successfully", id);
            return NoContent();
        }

        _logger.LogWarning("Failed to update member {MemberId}: {ErrorMessage}", id, result.ErrorMessage);
        return HandleResult(result);
    }

    /// <summary>
    /// Removes a member from a workplace.
    /// </summary>
    /// <param name="workplaceId">The unique identifier of the workplace.</param>
    /// <param name="id">The unique identifier of the member to remove.</param>
    /// <returns>NoContent on success, or error response.</returns>
    [HttpDelete("{workplaceId}/{id}")]
    public async Task<IActionResult> RemoveMember(Guid workplaceId, Guid id)
    {
        _logger.LogInformation("Removing member {MemberId} from workplace {WorkplaceId}", id, workplaceId);
        var result = await _memberService.RemoveMemberAsync(workplaceId, id);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Member {MemberId} removed successfully", id);
            return NoContent();
        }

        _logger.LogWarning("Failed to remove member {MemberId}: {ErrorMessage}", id, result.ErrorMessage);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves all workplaces that a user is a member of.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A list of workplaces the user belongs to.</returns>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult> GetUserWorkplaces(string userId)
    {
        _logger.LogInformation("Fetching workplaces for user {UserId}", userId);
        var result = await _memberService.GetWorkplacesByUserAsync(userId);
        return HandleResult(result);
    }

    /// <summary>
    /// Checks if a user is a manager of a specific workplace.
    /// </summary>
    /// <param name="workplaceId">The unique identifier of the workplace.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>Boolean indicating manager status.</returns>
    [HttpGet("is-manager/{workplaceId}/{userId}")]
    public async Task<ActionResult> IsUserManager(Guid workplaceId, string userId)
    {
        _logger.LogDebug("Checking if user {UserId} is manager of workplace {WorkplaceId}", userId, workplaceId);
        var result = await _memberService.IsUserManagerAsync(userId, workplaceId);
        return HandleResult(result);
    }

    #region Helper Methods

    /// <summary>
    /// Gets the current authenticated user's ID from the claims.
    /// </summary>
    /// <returns>The user ID if authenticated, otherwise null.</returns>
    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    /// <summary>
    /// Handles service result and returns appropriate HTTP response for generic results.
    /// </summary>
    /// <typeparam name="T">The type of data in the result.</typeparam>
    /// <param name="result">The service result to handle.</param>
    /// <returns>Appropriate HTTP response based on result status.</returns>
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

    /// <summary>
    /// Handles service result and returns appropriate HTTP response.
    /// </summary>
    /// <param name="result">The service result to handle.</param>
    /// <returns>Appropriate HTTP response based on result status.</returns>
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
