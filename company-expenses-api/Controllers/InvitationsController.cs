using CompanyExpenses.Database.Data;
using CompanyExpenses.Models.Entities;
using CompanyExpenses.Models.Enums;
using CompanyExpenses.Api.Services;
using CompanyExpenses.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace CompanyExpenses.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvitationsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<InvitationsController> _logger;
    private readonly IEmailService _emailService;

    public InvitationsController(AppDbContext context, ILogger<InvitationsController> logger, IEmailService emailService)
    {
        _context = context;
        _logger = logger;
        _emailService = emailService;
    }

    /// <summary>
    /// Gets all invitations (for admin)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvitationDto>>> GetInvitations()
    {
        var invitations = await _context.Invitations
            .Include(i => i.Workplace)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return invitations.Select(i => new InvitationDto
        {
            Id = i.Id,
            Email = i.Email,
            InvitedRoleId = i.InvitedRoleId,
            WorkplaceId = i.WorkplaceId,
            Token = i.Token,
            ExpiresAt = i.ExpiresAt,
            AcceptedAt = i.AcceptedAt,
            InvitedByUserId = i.InvitedByUserId,
            Status = i.Status,
            CreatedAt = i.CreatedAt,
            CreatedBy = i.CreatedBy,
            Workplace = i.Workplace != null ? new InvitationWorkplaceDto
            {
                Id = i.Workplace.Id,
                Name = i.Workplace.Name,
                Code = i.Workplace.Code,
                IsActive = i.Workplace.IsActive
            } : null
        }).ToList();
    }

    /// <summary>
    /// Gets invitation by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<InvitationDto>> GetInvitation(Guid id)
    {
        var invitation = await _context.Invitations
            .Include(i => i.Workplace)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invitation == null)
        {
            return NotFound();
        }

        return new InvitationDto
        {
            Id = invitation.Id,
            Email = invitation.Email,
            InvitedRoleId = invitation.InvitedRoleId,
            WorkplaceId = invitation.WorkplaceId,
            Token = invitation.Token,
            ExpiresAt = invitation.ExpiresAt,
            AcceptedAt = invitation.AcceptedAt,
            InvitedByUserId = invitation.InvitedByUserId,
            Status = invitation.Status,
            CreatedAt = invitation.CreatedAt,
            CreatedBy = invitation.CreatedBy,
            Workplace = invitation.Workplace != null ? new InvitationWorkplaceDto
            {
                Id = invitation.Workplace.Id,
                Name = invitation.Workplace.Name,
                Code = invitation.Workplace.Code,
                IsActive = invitation.Workplace.IsActive
            } : null
        };
    }

    /// <summary>
    /// Verifies invitation by token (used during registration)
    /// </summary>
    [HttpGet("verify/{token}")]
    public async Task<ActionResult<InvitationDto>> VerifyInvitation(string token)
    {
        var invitation = await _context.Invitations
            .Include(i => i.Workplace)
            .FirstOrDefaultAsync(i => i.Token == token);

        if (invitation == null)
        {
            return NotFound(new { message = "Invitation not found" });
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            return BadRequest(new { message = "Invitation has already been used" });
        }

        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            invitation.Status = InvitationStatus.Expired;
            await _context.SaveChangesAsync();
            return BadRequest(new { message = "Invitation has expired" });
        }

        return new InvitationDto
        {
            Id = invitation.Id,
            Email = invitation.Email,
            InvitedRoleId = invitation.InvitedRoleId,
            WorkplaceId = invitation.WorkplaceId,
            Token = invitation.Token,
            ExpiresAt = invitation.ExpiresAt,
            AcceptedAt = invitation.AcceptedAt,
            InvitedByUserId = invitation.InvitedByUserId,
            Status = invitation.Status,
            CreatedAt = invitation.CreatedAt,
            CreatedBy = invitation.CreatedBy,
            Workplace = invitation.Workplace != null ? new InvitationWorkplaceDto
            {
                Id = invitation.Workplace.Id,
                Name = invitation.Workplace.Name,
                Code = invitation.Workplace.Code,
                IsActive = invitation.Workplace.IsActive
            } : null
        };
    }

    /// <summary>
    /// Creates a new invitation
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<InvitationDto>> CreateInvitation(CreateInvitationDto dto)
    {
        // Check if email is already invited
        var existingInvitation = await _context.Invitations
            .FirstOrDefaultAsync(i => i.Email == dto.Email && i.Status == InvitationStatus.Pending);

        if (existingInvitation != null)
        {
            return BadRequest(new { message = "User with this email already has a pending invitation" });
        }

        // Check if workplace exists (if specified)
        Workplace? workplace = null;
        if (dto.WorkplaceId.HasValue)
        {
            workplace = await _context.Workplaces.FindAsync(dto.WorkplaceId.Value);
            if (workplace == null)
            {
                return BadRequest(new { message = "Workplace not found" });
            }
        }

        var invitation = new Invitation
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            InvitedRoleId = dto.InvitedRoleId,
            WorkplaceId = dto.WorkplaceId,
            Token = dto.Token ?? GenerateSecureToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7), // Valid for 7 days
            Status = InvitationStatus.Pending,
            InvitedByUserId = "test-user", // TODO: Získat z authentication
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-user"
        };

        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync();

        // Send invitation email
        try
        {
            await _emailService.SendInvitationEmailAsync(invitation.Email, invitation.Token, workplace?.Name);
            _logger.LogInformation("Created invitation for {Email}, token: {Token}, email sent", dto.Email, invitation.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send invitation email to {Email}", dto.Email);
            // Continue anyway - invitation is created
        }

        var invitationDto = new InvitationDto
        {
            Id = invitation.Id,
            Email = invitation.Email,
            InvitedRoleId = invitation.InvitedRoleId,
            WorkplaceId = invitation.WorkplaceId,
            Token = invitation.Token,
            ExpiresAt = invitation.ExpiresAt,
            AcceptedAt = invitation.AcceptedAt,
            InvitedByUserId = invitation.InvitedByUserId,
            Status = invitation.Status,
            CreatedAt = invitation.CreatedAt,
            CreatedBy = invitation.CreatedBy,
            Workplace = workplace != null ? new InvitationWorkplaceDto
            {
                Id = workplace.Id,
                Name = workplace.Name,
                Code = workplace.Code,
                IsActive = workplace.IsActive
            } : null
        };

        return CreatedAtAction(nameof(GetInvitation), new { id = invitation.Id }, invitationDto);
    }

    /// <summary>
    /// Marks invitation as accepted (called after registration is completed)
    /// </summary>
    [HttpPost("{id}/accept")]
    public async Task<IActionResult> AcceptInvitation(Guid id, [FromBody] AcceptInvitationDto dto)
    {
        var invitation = await _context.Invitations
            .Include(i => i.Workplace)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invitation == null)
        {
            return NotFound();
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            return BadRequest(new { message = "Invitation has already been used" });
        }

        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            invitation.Status = InvitationStatus.Expired;
            await _context.SaveChangesAsync();
            return BadRequest(new { message = "Invitation has expired" });
        }

        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedAt = DateTime.UtcNow;

        // If invitation has a workplace assigned, add user as member
        if (invitation.WorkplaceId.HasValue && !string.IsNullOrEmpty(dto.UserId))
        {
            var member = new WorkplaceMember
            {
                Id = Guid.NewGuid(),
                WorkplaceId = invitation.WorkplaceId.Value,
                UserId = dto.UserId,
                IsManager = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.UserId
            };

            _context.WorkplaceMembers.Add(member);
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Invitation successfully accepted" });
    }

    /// <summary>
    /// Cancels an invitation
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelInvitation(Guid id)
    {
        var invitation = await _context.Invitations.FindAsync(id);

        if (invitation == null)
        {
            return NotFound();
        }

        invitation.Status = InvitationStatus.Cancelled;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Resends invitation (refreshes token and expiration)
    /// </summary>
    [HttpPost("{id}/resend")]
    public async Task<ActionResult<InvitationDto>> ResendInvitation(Guid id)
    {
        var invitation = await _context.Invitations
            .Include(i => i.Workplace)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invitation == null)
        {
            return NotFound();
        }

        if (invitation.Status == InvitationStatus.Accepted)
        {
            return BadRequest(new { message = "Cannot resend an accepted invitation" });
        }

        // Refresh token and expiration
        invitation.Token = GenerateSecureToken();
        invitation.ExpiresAt = DateTime.UtcNow.AddDays(7);
        invitation.Status = InvitationStatus.Pending;

        await _context.SaveChangesAsync();

        // Send invitation email
        try
        {
            await _emailService.SendInvitationEmailAsync(invitation.Email, invitation.Token, invitation.Workplace?.Name);
            _logger.LogInformation("Resent invitation for {Email}, new token: {Token}, email sent", invitation.Email, invitation.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send invitation email to {Email}", invitation.Email);
            // Continue anyway - invitation is updated
        }

        return new InvitationDto
        {
            Id = invitation.Id,
            Email = invitation.Email,
            InvitedRoleId = invitation.InvitedRoleId,
            WorkplaceId = invitation.WorkplaceId,
            Token = invitation.Token,
            ExpiresAt = invitation.ExpiresAt,
            AcceptedAt = invitation.AcceptedAt,
            InvitedByUserId = invitation.InvitedByUserId,
            Status = invitation.Status,
            CreatedAt = invitation.CreatedAt,
            CreatedBy = invitation.CreatedBy,
            Workplace = invitation.Workplace != null ? new InvitationWorkplaceDto
            {
                Id = invitation.Workplace.Id,
                Name = invitation.Workplace.Name,
                Code = invitation.Workplace.Code,
                IsActive = invitation.Workplace.IsActive
            } : null
        };
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}

// DTOs
public class InvitationDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? InvitedRoleId { get; set; }
    public Guid? WorkplaceId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public string InvitedByUserId { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public InvitationWorkplaceDto? Workplace { get; set; }
}

public class InvitationWorkplaceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; }
}

public class CreateInvitationDto
{
    public string Email { get; set; } = string.Empty;
    public string? InvitedRoleId { get; set; }
    public string? Token { get; set; }
    public Guid? WorkplaceId { get; set; }

}

public class AcceptInvitationDto
{
    public string UserId { get; set; } = string.Empty;
}
