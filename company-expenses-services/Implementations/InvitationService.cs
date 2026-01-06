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
/// Invitation business service implementation
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

    public async Task<ServiceResult<IEnumerable<InvitationDto>>> GetAllInvitationsAsync()
    {
        var invitations = await _invitationRepository.GetAllWithWorkplaceAsync();

        var result = invitations.Select(MapToDto);
        return ServiceResult<IEnumerable<InvitationDto>>.Success(result);
    }

    public async Task<ServiceResult<InvitationDto>> GetInvitationByIdAsync(Guid id)
    {
        var invitation = await _invitationRepository.GetByIdWithWorkplaceAsync(id);
        if (invitation == null)
            return ServiceResult<InvitationDto>.NotFound("Invitation not found");

        return ServiceResult<InvitationDto>.Success(MapToDto(invitation));
    }

    public async Task<ServiceResult<InvitationDto>> VerifyInvitationAsync(string token)
    {
        var invitation = await _invitationRepository.GetByTokenAsync(token);
        if (invitation == null)
            return ServiceResult<InvitationDto>.NotFound("Invitation not found");

        if (invitation.Status != InvitationStatus.Pending)
            return ServiceResult<InvitationDto>.BadRequest("Invitation has already been used");

        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            invitation.Status = InvitationStatus.Expired;
            await _invitationRepository.SaveChangesAsync();
            return ServiceResult<InvitationDto>.BadRequest("Invitation has expired");
        }

        return ServiceResult<InvitationDto>.Success(MapToDto(invitation));
    }

    public async Task<ServiceResult<InvitationDto>> CreateInvitationAsync(CreateInvitationDto dto, string userId)
    {
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

    public async Task<ServiceResult> AcceptInvitationAsync(Guid id, string userId)
    {
        var invitation = await _invitationRepository.GetByIdWithWorkplaceAsync(id);
        if (invitation == null)
            return ServiceResult.NotFound("Invitation not found");

        if (invitation.Status != InvitationStatus.Pending)
            return ServiceResult.BadRequest("Invitation has already been used");

        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
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

    public async Task<ServiceResult> CancelInvitationAsync(Guid id)
    {
        var invitation = await _invitationRepository.GetByIdAsync(id);
        if (invitation == null)
            return ServiceResult.NotFound("Invitation not found");

        invitation.Status = InvitationStatus.Cancelled;
        await _invitationRepository.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteInvitationAsync(Guid id)
    {
        var invitation = await _invitationRepository.GetByIdAsync(id);
        if (invitation == null)
            return ServiceResult.NotFound("Invitation not found");

        _invitationRepository.Remove(invitation);
        await _invitationRepository.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<InvitationDto>> ResendInvitationAsync(Guid id)
    {
        var invitation = await _invitationRepository.GetByIdWithWorkplaceAsync(id);
        if (invitation == null)
            return ServiceResult<InvitationDto>.NotFound("Invitation not found");

        if (invitation.Status == InvitationStatus.Accepted)
            return ServiceResult<InvitationDto>.BadRequest("Cannot resend an accepted invitation");

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

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}
