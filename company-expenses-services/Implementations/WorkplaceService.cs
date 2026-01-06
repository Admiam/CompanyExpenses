using CompanyExpenses.Database.Repositories;
using CompanyExpenses.Models.Entities;
using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;
using CompanyExpenses.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompanyExpenses.Services.Implementations;

/// <summary>
/// Workplace business service implementation
/// </summary>
public class WorkplaceService : IWorkplaceService
{
    private readonly IWorkplaceRepository _workplaceRepository;
    private readonly ILogger<WorkplaceService> _logger;

    public WorkplaceService(
        IWorkplaceRepository workplaceRepository,
        ILogger<WorkplaceService> logger)
    {
        _workplaceRepository = workplaceRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<IEnumerable<WorkplaceDto>>> GetAllWorkplacesAsync()
    {
        var workplaces = await _workplaceRepository.GetAllWithMembersAsync();

        var result = workplaces.Select(w => new WorkplaceDto
        {
            Id = w.Id,
            Name = w.Name,
            Code = w.Code,
            IsActive = w.IsActive,
            CreatedAt = w.CreatedAt,
            CreatedBy = w.CreatedBy,
            Members = w.Members.Select(m => new WorkplaceMemberDto
            {
                Id = m.Id,
                WorkplaceId = m.WorkplaceId,
                UserId = m.UserId,
                PositionName = m.PositionName,
                IsManager = m.IsManager,
                CreatedAt = m.CreatedAt,
                CreatedBy = m.CreatedBy
            }).ToList()
        });

        return ServiceResult<IEnumerable<WorkplaceDto>>.Success(result);
    }

    public async Task<ServiceResult<WorkplaceDetailDto>> GetWorkplaceByIdAsync(Guid id)
    {
        var workplace = await _workplaceRepository.GetByIdWithDetailsAsync(id);
        if (workplace == null)
            return ServiceResult<WorkplaceDetailDto>.NotFound("Workplace not found");

        var result = new WorkplaceDetailDto
        {
            Id = workplace.Id,
            Name = workplace.Name,
            Code = workplace.Code,
            IsActive = workplace.IsActive,
            CreatedAt = workplace.CreatedAt,
            CreatedBy = workplace.CreatedBy,
            Members = workplace.Members.Select(m => new WorkplaceMemberDto
            {
                Id = m.Id,
                WorkplaceId = m.WorkplaceId,
                UserId = m.UserId,
                PositionName = m.PositionName,
                IsManager = m.IsManager,
                CreatedAt = m.CreatedAt,
                CreatedBy = m.CreatedBy
            }).ToList(),
            Limits = workplace.Limits.Select(l => new WorkplaceLimitDto
            {
                Id = l.Id,
                WorkplaceId = l.WorkplaceId,
                PeriodFrom = l.PeriodFrom,
                PeriodTo = l.PeriodTo,
                LimitAmount = l.LimitAmount,
                Currency = l.Currency,
                CategoryId = l.CategoryId,
                IsActive = l.IsActive,
                CreatedAt = l.CreatedAt,
                CreatedBy = l.CreatedBy
            }).ToList()
        };

        return ServiceResult<WorkplaceDetailDto>.Success(result);
    }

    public async Task<ServiceResult<WorkplaceDto>> CreateWorkplaceAsync(CreateWorkplaceDto dto, string userId)
    {
        var workplace = new Workplace
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Code = dto.Code,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await _workplaceRepository.AddAsync(workplace);
        await _workplaceRepository.SaveChangesAsync();

        _logger.LogInformation("Workplace created: {WorkplaceId} by user {UserId}", workplace.Id, userId);

        return ServiceResult<WorkplaceDto>.Success(new WorkplaceDto
        {
            Id = workplace.Id,
            Name = workplace.Name,
            Code = workplace.Code,
            IsActive = workplace.IsActive,
            CreatedAt = workplace.CreatedAt,
            CreatedBy = workplace.CreatedBy,
            Members = new List<WorkplaceMemberDto>()
        });
    }

    public async Task<ServiceResult> UpdateWorkplaceAsync(Guid id, UpdateWorkplaceDto dto)
    {
        var workplace = await _workplaceRepository.GetByIdAsync(id);
        if (workplace == null)
            return ServiceResult.NotFound("Workplace not found");

        workplace.Name = dto.Name;
        workplace.Code = dto.Code;
        workplace.IsActive = dto.IsActive;

        _workplaceRepository.Update(workplace);
        await _workplaceRepository.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<WorkplaceDependenciesDto>> GetDependenciesAsync(Guid id)
    {
        var workplace = await _workplaceRepository.GetByIdAsync(id);
        if (workplace == null)
            return ServiceResult<WorkplaceDependenciesDto>.NotFound("Workplace not found");

        var deps = await _workplaceRepository.GetDependenciesAsync(id);

        return ServiceResult<WorkplaceDependenciesDto>.Success(new WorkplaceDependenciesDto
        {
            WorkplaceId = id,
            MembersCount = deps.MembersCount,
            LimitsCount = deps.LimitsCount,
            InvitationsCount = deps.InvitationsCount,
            ExpensesCount = deps.ExpensesCount,
            CanDelete = deps.CanDelete
        });
    }

    public async Task<ServiceResult> DeleteWorkplaceAsync(Guid id)
    {
        var workplace = await _workplaceRepository.GetByIdAsync(id);
        if (workplace == null)
            return ServiceResult.NotFound("Workplace not found");

        var deps = await _workplaceRepository.GetDependenciesAsync(id);
        if (!deps.CanDelete)
        {
            return ServiceResult.BadRequest($"Cannot delete workplace with existing dependencies. " +
                $"Members: {deps.MembersCount}, Limits: {deps.LimitsCount}, " +
                $"Invitations: {deps.InvitationsCount}, Expenses: {deps.ExpensesCount}");
        }

        _workplaceRepository.Remove(workplace);
        await _workplaceRepository.SaveChangesAsync();

        _logger.LogInformation("Workplace deleted: {WorkplaceId}", id);
        return ServiceResult.Success();
    }
}
