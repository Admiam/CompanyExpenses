using CompanyExpenses.Database.Repositories;
using CompanyExpenses.Models.Entities;
using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;
using CompanyExpenses.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompanyExpenses.Services.Implementations;

/// <summary>
/// Workplace member business service implementation
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

    public async Task<ServiceResult<IEnumerable<WorkplaceMemberDto>>> GetMembersByWorkplaceAsync(Guid workplaceId)
    {
        var workplace = await _workplaceRepository.GetByIdAsync(workplaceId);
        if (workplace == null)
            return ServiceResult<IEnumerable<WorkplaceMemberDto>>.NotFound("Workplace not found");

        var members = await _memberRepository.GetByWorkplaceIdAsync(workplaceId);
        var result = members.Select(MapToDto);

        return ServiceResult<IEnumerable<WorkplaceMemberDto>>.Success(result);
    }

    public async Task<ServiceResult<WorkplaceMemberDto>> GetMemberByIdAsync(Guid workplaceId, Guid memberId)
    {
        var workplace = await _workplaceRepository.GetByIdAsync(workplaceId);
        if (workplace == null)
            return ServiceResult<WorkplaceMemberDto>.NotFound("Workplace not found");

        var member = await _memberRepository.GetByIdAsync(memberId);
        if (member == null || member.WorkplaceId != workplaceId)
            return ServiceResult<WorkplaceMemberDto>.NotFound("Member not found");

        return ServiceResult<WorkplaceMemberDto>.Success(MapToDto(member));
    }

    public async Task<ServiceResult<WorkplaceMemberDto>> AddMemberAsync(Guid workplaceId, CreateWorkplaceMemberDto dto, string userId)
    {
        var workplace = await _workplaceRepository.GetByIdAsync(workplaceId);
        if (workplace == null)
            return ServiceResult<WorkplaceMemberDto>.NotFound("Workplace not found");

        // Check if user is already a member
        var existingMember = await _memberRepository.GetByUserAndWorkplaceAsync(dto.UserId, workplaceId);
        if (existingMember != null)
            return ServiceResult<WorkplaceMemberDto>.BadRequest("User is already a member of this workplace");

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

    public async Task<ServiceResult> UpdateMemberAsync(Guid workplaceId, Guid memberId, UpdateWorkplaceMemberDto dto)
    {
        var workplace = await _workplaceRepository.GetByIdAsync(workplaceId);
        if (workplace == null)
            return ServiceResult.NotFound("Workplace not found");

        var member = await _memberRepository.GetByIdAsync(memberId);
        if (member == null || member.WorkplaceId != workplaceId)
            return ServiceResult.NotFound("Member not found");

        member.PositionName = dto.PositionName;
        member.IsManager = dto.IsManager;

        _memberRepository.Update(member);
        await _memberRepository.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> RemoveMemberAsync(Guid workplaceId, Guid memberId)
    {
        var workplace = await _workplaceRepository.GetByIdAsync(workplaceId);
        if (workplace == null)
            return ServiceResult.NotFound("Workplace not found");

        var member = await _memberRepository.GetByIdAsync(memberId);
        if (member == null || member.WorkplaceId != workplaceId)
            return ServiceResult.NotFound("Member not found");

        _memberRepository.Remove(member);
        await _memberRepository.SaveChangesAsync();

        _logger.LogInformation("Member {MemberId} removed from workplace {WorkplaceId}", memberId, workplaceId);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult<IEnumerable<WorkplaceDto>>> GetWorkplacesByUserAsync(string userId)
    {
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

    public async Task<ServiceResult<bool>> IsUserManagerAsync(string userId, Guid workplaceId)
    {
        var member = await _memberRepository.GetByUserAndWorkplaceAsync(userId, workplaceId);
        var isManager = member?.IsManager ?? false;
        return ServiceResult<bool>.Success(isManager);
    }

    // These methods are implemented directly in the API controller since they need AuthDbContext
    public Task<ServiceResult<IEnumerable<UserWithStatsDto>>> GetUsersWithStatsAsync(bool includeInactive)
    {
        throw new NotImplementedException("This method is implemented in the API controller");
    }

    public Task<ServiceResult<IEnumerable<UserWithStatsDto>>> GetInactiveUsersAsync()
    {
        throw new NotImplementedException("This method is implemented in the API controller");
    }

    public Task<ServiceResult<UserDetailDto>> GetUserDetailAsync(string userId)
    {
        throw new NotImplementedException("This method is implemented in the API controller");
    }

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
