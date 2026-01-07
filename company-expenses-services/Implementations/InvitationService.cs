using CompanyExpenses.Database.Repositories;
using CompanyExpenses.Models.Entities;
using CompanyExpenses.Models.Enums;
using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;
using CompanyExpenses.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace CompanyExpenses.Services.Implementations;

/// <summary>
/// Service implementation for invitation management including creation, verification, and acceptance workflows.
/// </summary>
public class InvitationService : IInvitationService
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IWorkplaceRepository _workplaceRepository;
    private readonly IWorkplaceMemberRepository _memberRepository;
    private readonly IEmailService _emailService;
    private readonly int _invitationExpirationDays;
    private readonly ILogger<InvitationService> _logger;

    public InvitationService(
        IInvitationRepository invitationRepository,
        IWorkplaceRepository workplaceRepository,
        IWorkplaceMemberRepository memberRepository,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<InvitationService> logger)
    {
        _invitationRepository = invitationRepository;
        _workplaceRepository = workplaceRepository;
        _memberRepository = memberRepository;
        _emailService = emailService;
        _invitationExpirationDays = configuration.GetValue<int>("InvitationSettings:ExpirationDays", 7);
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all invitations with workplace information.
    /// </summary>
    /// <returns>A list of all invitations.</returns>
    public async Task<ServiceResult<IEnumerable<InvitationDto>>> GetAllInvitationsAsync()
    {
        _logger.LogInformation("Fetching all invitations");
        var invitations = await _invitationRepository.GetAllWithWorkplaceAsync();

        var result = invitations.Select(MapToDto);
        return ServiceResult<IEnumerable<InvitationDto>>.Success(result);
    }

    /// <summary>
    /// Retrieves a specific invitation by ID.
    /// </summary>
    /// <param name="id">The invitation ID.</param>
    /// <returns>The invitation if found, otherwise NotFound.</returns>
    public async Task<ServiceResult<InvitationDto>> GetInvitationByIdAsync(Guid id)
    {
        _logger.LogInformation("Fetching invitation {InvitationId}", id);
        var invitation = await _invitationRepository.GetByIdWithWorkplaceAsync(id);
        if (invitation == null)
        {
            _logger.LogWarning("Invitation not found: {InvitationId}", id);
            return ServiceResult<InvitationDto>.NotFound("Invitation not found");
        }

        return ServiceResult<InvitationDto>.Success(MapToDto(invitation));
    }

    /// <summary>
    /// Verifies an invitation token and checks if it's still valid.
    /// </summary>
    /// <param name="token">The invitation token.</param>
    /// <returns>The invitation if valid, or error status.</returns>
    public async Task<ServiceResult<InvitationDto>> VerifyInvitationAsync(string token)
    {
        _logger.LogInformation("Verifying invitation token");
        var invitation = await _invitationRepository.GetByTokenAsync(token);
        if (invitation == null)
        {
            _logger.LogWarning("Invalid invitation token");
            return ServiceResult<InvitationDto>.NotFound("Invitation not found");
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            _logger.LogWarning("Invitation already used: {InvitationId}", invitation.Id);
            return ServiceResult<InvitationDto>.BadRequest("Invitation has already been used");
        }

        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Invitation expired: {InvitationId}", invitation.Id);
            invitation.Status = InvitationStatus.Expired;
            await _invitationRepository.SaveChangesAsync();
            return ServiceResult<InvitationDto>.BadRequest("Invitation has expired");
        }

        return ServiceResult<InvitationDto>.Success(MapToDto(invitation));
    }

    /// <summary>
    /// Creates a new invitation and sends an email to the invitee.
    /// </summary>
    /// <param name="dto">The invitation creation data.</param>
    /// <param name="userId">The ID of the user sending the invitation.</param>
    /// <returns>The created invitation.</returns>
    public async Task<ServiceResult<InvitationDto>> CreateInvitationAsync(CreateInvitationDto dto, string userId)
    {
        _logger.LogInformation("Creating invitation for {Email} by user {UserId}", dto.Email, userId);
        // Check for existing pending invitation
        if (await _invitationRepository.HasPendingInvitationAsync(dto.Email))
            return ServiceResult<InvitationDto>.BadRequest("User with this email already has a pending invitation");

        // Validate workplace if specified
        Workplace? workplace = null;
        if (dto.WorkplaceId.HasValue)
        {
            workplace = await _workplaceRepository.GetByIdAsync(dto.WorkplaceId.Value);
            if (workplace == null)
                return ServiceResult<InvitationDto>.BadRequest("Workplace not found");
        }

        var invitation = new Invitation
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            InvitedRoleId = dto.InvitedRoleId,
            WorkplaceId = dto.WorkplaceId,
            Token = dto.Token ?? GenerateSecureToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(_invitationExpirationDays),
            Status = InvitationStatus.Pending,
            InvitedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await _invitationRepository.AddAsync(invitation);
        await _invitationRepository.SaveChangesAsync();

        // Send invitation email
        try
        {
            await _emailService.SendInvitationEmailAsync(invitation.Email, invitation.Token, workplace?.Name);
            _logger.LogInformation("Created invitation for {Email}, token: {Token}, email sent", dto.Email, invitation.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send invitation email to {Email}", dto.Email);
        }

        invitation.Workplace = workplace;
        return ServiceResult<InvitationDto>.Success(MapToDto(invitation));
    }

    /// <summary>
    /// Accepts an invitation and optionally adds the user to the associated workplace.
    /// </summary>
    /// <param name="id">The invitation ID.</param>
    /// <param name="userId">The ID of the accepting user.</param>
    /// <returns>Success or failure result.</returns>
    public async Task<ServiceResult> AcceptInvitationAsync(Guid id, string userId)
    {
        _logger.LogInformation("Accepting invitation {InvitationId} by user {UserId}", id, userId);
        var invitation = await _invitationRepository.GetByIdWithWorkplaceAsync(id);
        if (invitation == null)
        {
            _logger.LogWarning("Invitation not found: {InvitationId}", id);
            return ServiceResult.NotFound("Invitation not found");
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            _logger.LogWarning("Invitation already used: {InvitationId}", id);
            return ServiceResult.BadRequest("Invitation has already been used");
        }

        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Invitation expired: {InvitationId}", id);
            invitation.Status = InvitationStatus.Expired;
            await _invitationRepository.SaveChangesAsync();
            return ServiceResult.BadRequest("Invitation has expired");
        }

        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedAt = DateTime.UtcNow;

        // Add user to workplace if specified
        if (invitation.WorkplaceId.HasValue && !string.IsNullOrEmpty(userId))
        {
            var member = new WorkplaceMember
            {
                Id = Guid.NewGuid(),
                WorkplaceId = invitation.WorkplaceId.Value,
                UserId = userId,
                IsManager = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _memberRepository.AddAsync(member);
        }

        await _invitationRepository.SaveChangesAsync();

        _logger.LogInformation("Invitation {InvitationId} accepted by user {UserId}", id, userId);
        return ServiceResult.Success();
    }

    /// <summary>
    /// Cancels a pending invitation.
    /// </summary>
    /// <param name="id">The invitation ID.</param>
    /// <returns>Success or failure result.</returns>
    public async Task<ServiceResult> CancelInvitationAsync(Guid id)
    {
        _logger.LogInformation("Cancelling invitation {InvitationId}", id);
        var invitation = await _invitationRepository.GetByIdAsync(id);
        if (invitation == null)
        {
            _logger.LogWarning("Invitation not found: {InvitationId}", id);
            return ServiceResult.NotFound("Invitation not found");
        }

        invitation.Status = InvitationStatus.Cancelled;
        await _invitationRepository.SaveChangesAsync();

        _logger.LogInformation("Invitation cancelled: {InvitationId}", id);
        return ServiceResult.Success();
    }

    /// <summary>
    /// Permanently deletes an invitation from the database.
    /// </summary>
    /// <param name="id">The invitation ID.</param>
    /// <returns>Success or failure result.</returns>
    public async Task<ServiceResult> DeleteInvitationAsync(Guid id)
    {
        _logger.LogInformation("Deleting invitation {InvitationId}", id);
        var invitation = await _invitationRepository.GetByIdAsync(id);
        if (invitation == null)
        {
            _logger.LogWarning("Invitation not found: {InvitationId}", id);
            return ServiceResult.NotFound("Invitation not found");
        }

        _invitationRepository.Remove(invitation);
        await _invitationRepository.SaveChangesAsync();

        _logger.LogInformation("Invitation deleted: {InvitationId}", id);
        return ServiceResult.Success();
    }

    /// <summary>
    /// Resends an invitation with a refreshed token and expiration date.
    /// </summary>
    /// <param name="id">The invitation ID.</param>
    /// <returns>The updated invitation.</returns>
    public async Task<ServiceResult<InvitationDto>> ResendInvitationAsync(Guid id)
    {
        _logger.LogInformation("Resending invitation {InvitationId}", id);
        var invitation = await _invitationRepository.GetByIdWithWorkplaceAsync(id);
        if (invitation == null)
        {
            _logger.LogWarning("Invitation not found: {InvitationId}", id);
            return ServiceResult<InvitationDto>.NotFound("Invitation not found");
        }

        if (invitation.Status == InvitationStatus.Accepted)
        {
            _logger.LogWarning("Cannot resend accepted invitation: {InvitationId}", id);
            return ServiceResult<InvitationDto>.BadRequest("Cannot resend an accepted invitation");
        }

        // Refresh token and expiration
        invitation.Token = GenerateSecureToken();
        invitation.ExpiresAt = DateTime.UtcNow.AddDays(_invitationExpirationDays);
        invitation.Status = InvitationStatus.Pending;

        await _invitationRepository.SaveChangesAsync();

        // Send email
        try
        {
            await _emailService.SendInvitationEmailAsync(invitation.Email, invitation.Token, invitation.Workplace?.Name);
            _logger.LogInformation("Resent invitation for {Email}, new token: {Token}", invitation.Email, invitation.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send invitation email to {Email}", invitation.Email);
        }

        return ServiceResult<InvitationDto>.Success(MapToDto(invitation));
    }

    /// <summary>
    /// Maps an Invitation entity to its DTO representation.
    /// </summary>
    private static InvitationDto MapToDto(Invitation invitation) => new()
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

    /// <summary>
    /// Generates a cryptographically secure random token for invitation links.
    /// </summary>
    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}
