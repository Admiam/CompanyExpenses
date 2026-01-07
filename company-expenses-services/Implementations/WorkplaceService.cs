using CompanyExpenses.Database.Repositories;
using CompanyExpenses.Models.Entities;
using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;
using CompanyExpenses.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompanyExpenses.Services.Implementations;

/// <summary>
/// Service implementation for workplace management including CRUD operations and dependency checking.
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

    /// <summary>
    /// Retrieves all workplaces with their member information.
    /// </summary>
    /// <returns>A list of all workplaces with members.</returns>
    public async Task<ServiceResult<IEnumerable<WorkplaceDto>>> GetAllWorkplacesAsync()
    {
        _logger.LogInformation("Fetching all workplaces");
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

    /// <summary>
    /// Retrieves detailed information about a specific workplace including members and limits.
    /// </summary>
    /// <param name="id">The workplace ID.</param>
    /// <returns>Workplace details if found, otherwise NotFound.</returns>
    public async Task<ServiceResult<WorkplaceDetailDto>> GetWorkplaceByIdAsync(Guid id)
    {
        _logger.LogInformation("Fetching workplace details for ID: {WorkplaceId}", id);
        var workplace = await _workplaceRepository.GetByIdWithDetailsAsync(id);
        if (workplace == null)
        {
            _logger.LogWarning("Workplace not found: {WorkplaceId}", id);
            return ServiceResult<WorkplaceDetailDto>.NotFound("Workplace not found");
        }

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

    /// <summary>
    /// Creates a new workplace with the specified details.
    /// </summary>
    /// <param name="dto">The workplace creation data.</param>
    /// <param name="userId">The ID of the user creating the workplace.</param>
    /// <returns>The created workplace.</returns>
    public async Task<ServiceResult<WorkplaceDto>> CreateWorkplaceAsync(CreateWorkplaceDto dto, string userId)
    {
        _logger.LogInformation("Creating workplace '{Name}' by user {UserId}", dto.Name, userId);
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

    /// <summary>
    /// Updates an existing workplace's basic information.
    /// </summary>
    /// <param name="id">The workplace ID.</param>
    /// <param name="dto">The updated workplace data.</param>
    /// <returns>Success or failure result.</returns>
    public async Task<ServiceResult> UpdateWorkplaceAsync(Guid id, UpdateWorkplaceDto dto)
    {
        _logger.LogInformation("Updating workplace {WorkplaceId}", id);
        var workplace = await _workplaceRepository.GetByIdAsync(id);
        if (workplace == null)
        {
            _logger.LogWarning("Workplace not found: {WorkplaceId}", id);
            return ServiceResult.NotFound("Workplace not found");
        }

        workplace.Name = dto.Name;
        workplace.Code = dto.Code;
        workplace.IsActive = dto.IsActive;

        _workplaceRepository.Update(workplace);
        await _workplaceRepository.SaveChangesAsync();

        _logger.LogInformation("Workplace updated successfully: {WorkplaceId}", id);
        return ServiceResult.Success();
    }

    /// <summary>
    /// Gets dependency information for a workplace to determine if it can be deleted.
    /// </summary>
    /// <param name="id">The workplace ID.</param>
    /// <returns>Dependency counts and deletion eligibility.</returns>
    public async Task<ServiceResult<WorkplaceDependenciesDto>> GetDependenciesAsync(Guid id)
    {
        _logger.LogInformation("Checking dependencies for workplace {WorkplaceId}", id);
        var workplace = await _workplaceRepository.GetByIdAsync(id);
        if (workplace == null)
        {
            _logger.LogWarning("Workplace not found: {WorkplaceId}", id);
            return ServiceResult<WorkplaceDependenciesDto>.NotFound("Workplace not found");
        }

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

    /// <summary>
    /// Deletes a workplace if it has no dependencies (members, limits, invitations, expenses).
    /// </summary>
    /// <param name="id">The workplace ID.</param>
    /// <returns>Success or failure result.</returns>
    public async Task<ServiceResult> DeleteWorkplaceAsync(Guid id)
    {
        _logger.LogInformation("Attempting to delete workplace {WorkplaceId}", id);
        var workplace = await _workplaceRepository.GetByIdAsync(id);
        if (workplace == null)
        {
            _logger.LogWarning("Workplace not found: {WorkplaceId}", id);
            return ServiceResult.NotFound("Workplace not found");
        }

        var deps = await _workplaceRepository.GetDependenciesAsync(id);
        if (!deps.CanDelete)
        {
            _logger.LogWarning("Cannot delete workplace {WorkplaceId} - has dependencies: Members={MembersCount}, Limits={LimitsCount}, Invitations={InvitationsCount}, Expenses={ExpensesCount}",
                id, deps.MembersCount, deps.LimitsCount, deps.InvitationsCount, deps.ExpensesCount);
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
