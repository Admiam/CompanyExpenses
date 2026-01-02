using CompanyExpenses.Database.Data;
using CompanyExpenses.Models.Entities;
using CompanyExpenses.Models.Enums;
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

    public InvitationsController(AppDbContext context, ILogger<InvitationsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all invitations (for admin)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Invitation>>> GetInvitations()
    {
        return await _context.Invitations
            .Include(i => i.Workplace)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets invitation by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Invitation>> GetInvitation(Guid id)
    {
        var invitation = await _context.Invitations
            .Include(i => i.Workplace)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invitation == null)
        {
            return NotFound();
        }

        return invitation;
    }

    /// <summary>
    /// Verifies invitation by token (used during registration)
    /// </summary>
    [HttpGet("verify/{token}")]
    public async Task<ActionResult<Invitation>> VerifyInvitation(string token)
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

        return invitation;
    }

    /// <summary>
    /// Creates a new invitation
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Invitation>> CreateInvitation(CreateInvitationDto dto)
    {
        // Check if email is already invited
        var existingInvitation = await _context.Invitations
            .FirstOrDefaultAsync(i => i.Email == dto.Email && i.Status == InvitationStatus.Pending);

        if (existingInvitation != null)
        {
            return BadRequest(new { message = "User with this email already has a pending invitation" });
        }

        // Check if workplace exists (if specified)
        if (dto.WorkplaceId.HasValue)
        {
            var workplaceExists = await _context.Workplaces.AnyAsync(w => w.Id == dto.WorkplaceId.Value);
            if (!workplaceExists)
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
            Token = GenerateSecureToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7), // Valid for 7 days
            Status = InvitationStatus.Pending,
            InvitedByUserId = "test-user", // TODO: Získat z authentication
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-user"
        };

        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync();

        // TODO: Send email with registration link
        _logger.LogInformation("Created invitation for {Email}, token: {Token}", dto.Email, invitation.Token);

        return CreatedAtAction(nameof(GetInvitation), new { id = invitation.Id }, invitation);
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
    public async Task<ActionResult<Invitation>> ResendInvitation(Guid id)
    {
        var invitation = await _context.Invitations.FindAsync(id);

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

        // TODO: Send email
        _logger.LogInformation("Resent invitation for {Email}, new token: {Token}", invitation.Email, invitation.Token);

        return invitation;
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
public class CreateInvitationDto
{
    public string Email { get; set; } = string.Empty;
    public string? InvitedRoleId { get; set; }
    public Guid? WorkplaceId { get; set; }
}

public class AcceptInvitationDto
{
    public string UserId { get; set; } = string.Empty;
}
