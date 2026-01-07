using CompanyExpenses.Database.Repositories;
using CompanyExpenses.Models.Entities;
using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;
using CompanyExpenses.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompanyExpenses.Services.Implementations;

/// <summary>
/// Service implementation for workplace member management including adding, updating, and removing members.
/// </summary>
public class WorkplaceMemberService : IWorkplaceMemberService
{
    private readonly IWorkplaceMemberRepository _memberRepository;
    private readonly IWorkplaceRepository _workplaceRepository;
    private readonly ILogger<WorkplaceMemberService> _logger;

    public WorkplaceMemberService(
        IWorkplaceMemberRepository memberRepository,
        IWorkplaceRepository workplaceRepository,
        ILogger<WorkplaceMemberService> logger)
    {
        _memberRepository = memberRepository;
        _workplaceRepository = workplaceRepository;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all members of a specific workplace.
    /// </summary>
    /// <param name="workplaceId">The workplace ID.</param>
    /// <returns>A list of workplace members.</returns>
    public async Task<ServiceResult<IEnumerable<WorkplaceMemberDto>>> GetMembersByWorkplaceAsync(Guid workplaceId)
    {
        _logger.LogInformation("Fetching members for workplace {WorkplaceId}", workplaceId);
        var workplace = await _workplaceRepository.GetByIdAsync(workplaceId);
        if (workplace == null)
        {
            _logger.LogWarning("Workplace not found: {WorkplaceId}", workplaceId);
            return ServiceResult<IEnumerable<WorkplaceMemberDto>>.NotFound("Workplace not found");
        }

        var members = await _memberRepository.GetByWorkplaceIdAsync(workplaceId);
        var result = members.Select(MapToDto);

        return ServiceResult<IEnumerable<WorkplaceMemberDto>>.Success(result);
    }

    /// <summary>
    /// Retrieves a specific member by ID within a workplace.
    /// </summary>
    /// <param name="workplaceId">The workplace ID.</param>
    /// <param name="memberId">The member ID.</param>
    /// <returns>The member if found, otherwise NotFound.</returns>
    public async Task<ServiceResult<WorkplaceMemberDto>> GetMemberByIdAsync(Guid workplaceId, Guid memberId)
    {
        _logger.LogInformation("Fetching member {MemberId} from workplace {WorkplaceId}", memberId, workplaceId);
        var workplace = await _workplaceRepository.GetByIdAsync(workplaceId);
        if (workplace == null)
        {
            _logger.LogWarning("Workplace not found: {WorkplaceId}", workplaceId);
            return ServiceResult<WorkplaceMemberDto>.NotFound("Workplace not found");
        }

        var member = await _memberRepository.GetByIdAsync(memberId);
        if (member == null || member.WorkplaceId != workplaceId)
        {
            _logger.LogWarning("Member not found: {MemberId}", memberId);
            return ServiceResult<WorkplaceMemberDto>.NotFound("Member not found");
        }

        return ServiceResult<WorkplaceMemberDto>.Success(MapToDto(member));
    }

    /// <summary>
    /// Adds a new member to a workplace.
    /// </summary>
    /// <param name="workplaceId">The workplace ID.</param>
    /// <param name="dto">The member creation data.</param>
    /// <param name="userId">The ID of the user adding the member.</param>
    /// <returns>The created member.</returns>
    public async Task<ServiceResult<WorkplaceMemberDto>> AddMemberAsync(Guid workplaceId, CreateWorkplaceMemberDto dto, string userId)
    {
        _logger.LogInformation("Adding member {NewUserId} to workplace {WorkplaceId}", dto.UserId, workplaceId);
        var workplace = await _workplaceRepository.GetByIdAsync(workplaceId);
        if (workplace == null)
        {
            _logger.LogWarning("Workplace not found: {WorkplaceId}", workplaceId);
            return ServiceResult<WorkplaceMemberDto>.NotFound("Workplace not found");
        }

        var existingMember = await _memberRepository.GetByUserAndWorkplaceAsync(dto.UserId, workplaceId);
        if (existingMember != null)
        {
            _logger.LogWarning("User {UserId} is already a member of workplace {WorkplaceId}", dto.UserId, workplaceId);
            return ServiceResult<WorkplaceMemberDto>.BadRequest("User is already a member of this workplace");
        }

        var member = new WorkplaceMember
        {
            Id = Guid.NewGuid(),
            WorkplaceId = workplaceId,
            UserId = dto.UserId,
            PositionName = dto.PositionName,
            IsManager = dto.IsManager,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await _memberRepository.AddAsync(member);
        await _memberRepository.SaveChangesAsync();

        _logger.LogInformation("Member added to workplace {WorkplaceId}: User {UserId}", workplaceId, dto.UserId);

        return ServiceResult<WorkplaceMemberDto>.Success(MapToDto(member));
    }

    /// <summary>
    /// Updates a member's position and manager status.
    /// </summary>
    /// <param name="workplaceId">The workplace ID.</param>
    /// <param name="memberId">The member ID.</param>
    /// <param name="dto">The updated member data.</param>
    /// <returns>Success or failure result.</returns>
    public async Task<ServiceResult> UpdateMemberAsync(Guid workplaceId, Guid memberId, UpdateWorkplaceMemberDto dto)
    {
        _logger.LogInformation("Updating member {MemberId} in workplace {WorkplaceId}", memberId, workplaceId);
        var workplace = await _workplaceRepository.GetByIdAsync(workplaceId);
        if (workplace == null)
        {
            _logger.LogWarning("Workplace not found: {WorkplaceId}", workplaceId);
            return ServiceResult.NotFound("Workplace not found");
        }

        var member = await _memberRepository.GetByIdAsync(memberId);
        if (member == null || member.WorkplaceId != workplaceId)
        {
            _logger.LogWarning("Member not found: {MemberId}", memberId);
            return ServiceResult.NotFound("Member not found");
        }

        member.PositionName = dto.PositionName;
        member.IsManager = dto.IsManager;

        _memberRepository.Update(member);
        await _memberRepository.SaveChangesAsync();

        _logger.LogInformation("Member updated successfully: {MemberId}", memberId);
        return ServiceResult.Success();
    }

    /// <summary>
    /// Removes a member from a workplace.
    /// </summary>
    /// <param name="workplaceId">The workplace ID.</param>
    /// <param name="memberId">The member ID.</param>
    /// <returns>Success or failure result.</returns>
    public async Task<ServiceResult> RemoveMemberAsync(Guid workplaceId, Guid memberId)
    {
        _logger.LogInformation("Removing member {MemberId} from workplace {WorkplaceId}", memberId, workplaceId);
        var workplace = await _workplaceRepository.GetByIdAsync(workplaceId);
        if (workplace == null)
        {
            _logger.LogWarning("Workplace not found: {WorkplaceId}", workplaceId);
            return ServiceResult.NotFound("Workplace not found");
        }

        var member = await _memberRepository.GetByIdAsync(memberId);
        if (member == null || member.WorkplaceId != workplaceId)
        {
            _logger.LogWarning("Member not found: {MemberId}", memberId);
            return ServiceResult.NotFound("Member not found");
        }

        _memberRepository.Remove(member);
        await _memberRepository.SaveChangesAsync();

        _logger.LogInformation("Member {MemberId} removed from workplace {WorkplaceId}", memberId, workplaceId);
        return ServiceResult.Success();
    }

    /// <summary>
    /// Retrieves all workplaces where a user is a member.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A list of workplaces.</returns>
    public async Task<ServiceResult<IEnumerable<WorkplaceDto>>> GetWorkplacesByUserAsync(string userId)
    {
        _logger.LogInformation("Fetching workplaces for user {UserId}", userId);
        var workplaces = await _memberRepository.GetWorkplacesByUserIdAsync(userId);
        var result = workplaces.Select(w => new WorkplaceDto
        {
            Id = w.Id,
            Name = w.Name,
            Code = w.Code,
            IsActive = w.IsActive,
            CreatedAt = w.CreatedAt,
            CreatedBy = w.CreatedBy,
            Members = new List<WorkplaceMemberDto>()
        });

        return ServiceResult<IEnumerable<WorkplaceDto>>.Success(result);
    }

    /// <summary>
    /// Checks if a user has manager privileges in a specific workplace.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="workplaceId">The workplace ID.</param>
    /// <returns>True if user is a manager, false otherwise.</returns>
    public async Task<ServiceResult<bool>> IsUserManagerAsync(string userId, Guid workplaceId)
    {
        var member = await _memberRepository.GetByUserAndWorkplaceAsync(userId, workplaceId);
        var isManager = member?.IsManager ?? false;
        return ServiceResult<bool>.Success(isManager);
    }

    /// <summary>
    /// Not implemented - handled directly in API controller with AuthDbContext.
    /// </summary>
    public Task<ServiceResult<IEnumerable<UserWithStatsDto>>> GetUsersWithStatsAsync(bool includeInactive)
    {
        throw new NotImplementedException("This method is implemented in the API controller");
    }

    /// <summary>
    /// Not implemented - handled directly in API controller with AuthDbContext.
    /// </summary>
    public Task<ServiceResult<IEnumerable<UserWithStatsDto>>> GetInactiveUsersAsync()
    {
        throw new NotImplementedException("This method is implemented in the API controller");
    }

    /// <summary>
    /// Not implemented - handled directly in API controller with AuthDbContext.
    /// </summary>
    public Task<ServiceResult<UserDetailDto>> GetUserDetailAsync(string userId)
    {
        throw new NotImplementedException("This method is implemented in the API controller");
    }

    /// <summary>
    /// Maps a WorkplaceMember entity to its DTO representation.
    /// </summary>
    private static WorkplaceMemberDto MapToDto(WorkplaceMember member) => new()
    {
        Id = member.Id,
        WorkplaceId = member.WorkplaceId,
        UserId = member.UserId,
        PositionName = member.PositionName,
        IsManager = member.IsManager,
        CreatedAt = member.CreatedAt,
        CreatedBy = member.CreatedBy
    };
}
