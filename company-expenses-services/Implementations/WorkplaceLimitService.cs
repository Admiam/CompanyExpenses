using CompanyExpenses.Database.Repositories;
using CompanyExpenses.Models.Entities;
using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;
using CompanyExpenses.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompanyExpenses.Services.Implementations;

/// <summary>
/// Service implementation for workplace spending limit management including budget tracking and usage calculation.
/// </summary>
public class WorkplaceLimitService : IWorkplaceLimitService
{
    private readonly IWorkplaceLimitRepository _limitRepository;
    private readonly IWorkplaceRepository _workplaceRepository;
    private readonly ILogger<WorkplaceLimitService> _logger;

    public WorkplaceLimitService(
        IWorkplaceLimitRepository limitRepository,
        IWorkplaceRepository workplaceRepository,
        ILogger<WorkplaceLimitService> logger)
    {
        _limitRepository = limitRepository;
        _workplaceRepository = workplaceRepository;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all spending limits for a specific workplace.
    /// </summary>
    /// <param name="workplaceId">The workplace ID.</param>
    /// <returns>A list of workplace limits.</returns>
    public async Task<ServiceResult<IEnumerable<WorkplaceLimitDto>>> GetLimitsByWorkplaceAsync(Guid workplaceId)
    {
        _logger.LogInformation("Fetching limits for workplace {WorkplaceId}", workplaceId);
        var workplace = await _workplaceRepository.GetByIdAsync(workplaceId);
        if (workplace == null)
        {
            _logger.LogWarning("Workplace not found: {WorkplaceId}", workplaceId);
            return ServiceResult<IEnumerable<WorkplaceLimitDto>>.NotFound("Workplace not found");
        }

        var limits = await _limitRepository.GetByWorkplaceIdAsync(workplaceId);
        var result = limits.Select(MapToDto);

        return ServiceResult<IEnumerable<WorkplaceLimitDto>>.Success(result);
    }

    /// <summary>
    /// Retrieves a specific limit by ID within a workplace.
    /// </summary>
    /// <param name="workplaceId">The workplace ID.</param>
    /// <param name="limitId">The limit ID.</param>
    /// <returns>The limit if found, otherwise NotFound.</returns>
    public async Task<ServiceResult<WorkplaceLimitDto>> GetLimitByIdAsync(Guid workplaceId, Guid limitId)
    {
        _logger.LogInformation("Fetching limit {LimitId} from workplace {WorkplaceId}", limitId, workplaceId);
        var workplace = await _workplaceRepository.GetByIdAsync(workplaceId);
        if (workplace == null)
        {
            _logger.LogWarning("Workplace not found: {WorkplaceId}", workplaceId);
            return ServiceResult<WorkplaceLimitDto>.NotFound("Workplace not found");
        }

        var limit = await _limitRepository.GetByIdAsync(limitId);
        if (limit == null || limit.WorkplaceId != workplaceId)
        {
            _logger.LogWarning("Limit not found: {LimitId}", limitId);
            return ServiceResult<WorkplaceLimitDto>.NotFound("Limit not found");
        }

        return ServiceResult<WorkplaceLimitDto>.Success(MapToDto(limit));
    }

    /// <summary>
    /// Creates a new spending limit for a workplace category. Validates date ranges and checks for overlaps.
    /// </summary>
    /// <param name="workplaceId">The workplace ID.</param>
    /// <param name="dto">The limit creation data.</param>
    /// <param name="userId">The ID of the user creating the limit.</param>
    /// <returns>The created limit.</returns>
    public async Task<ServiceResult<WorkplaceLimitDto>> CreateLimitAsync(Guid workplaceId, CreateWorkplaceLimitDto dto, string userId)
    {
        _logger.LogInformation("Creating limit for workplace {WorkplaceId}, category {CategoryId}", workplaceId, dto.CategoryId);
        var workplace = await _workplaceRepository.GetByIdAsync(workplaceId);
        if (workplace == null)
        {
            _logger.LogWarning("Workplace not found: {WorkplaceId}", workplaceId);
            return ServiceResult<WorkplaceLimitDto>.NotFound("Workplace not found");
        }

        if (dto.PeriodFrom >= dto.PeriodTo)
        {
            _logger.LogWarning("Invalid period dates: {From} >= {To}", dto.PeriodFrom, dto.PeriodTo);
            return ServiceResult<WorkplaceLimitDto>.BadRequest("Period start date must be before end date");
        }

        var hasOverlap = await _limitRepository.HasOverlappingLimitAsync(
            workplaceId, dto.CategoryId, dto.PeriodFrom, dto.PeriodTo);

        if (hasOverlap)
        {
            _logger.LogWarning("Overlapping limit exists for category {CategoryId} in workplace {WorkplaceId}", dto.CategoryId, workplaceId);
            return ServiceResult<WorkplaceLimitDto>.BadRequest("A limit for this category already exists in the specified period");
        }

        var limit = new WorkplaceLimit
        {
            Id = Guid.NewGuid(),
            WorkplaceId = workplaceId,
            PeriodFrom = dto.PeriodFrom,
            PeriodTo = dto.PeriodTo,
            LimitAmount = dto.LimitAmount,
            Currency = dto.Currency,
            CategoryId = dto.CategoryId,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await _limitRepository.AddAsync(limit);
        await _limitRepository.SaveChangesAsync();

        _logger.LogInformation("Limit created for workplace {WorkplaceId}: {LimitAmount} {Currency}",
            workplaceId, dto.LimitAmount, dto.Currency);

        return ServiceResult<WorkplaceLimitDto>.Success(MapToDto(limit));
    }

    /// <summary>
    /// Updates an existing spending limit. Validates date ranges and checks for overlaps.
    /// </summary>
    /// <param name="workplaceId">The workplace ID.</param>
    /// <param name="limitId">The limit ID.</param>
    /// <param name="dto">The updated limit data.</param>
    /// <returns>Success or failure result.</returns>
    public async Task<ServiceResult> UpdateLimitAsync(Guid workplaceId, Guid limitId, UpdateWorkplaceLimitDto dto)
    {
        _logger.LogInformation("Updating limit {LimitId} in workplace {WorkplaceId}", limitId, workplaceId);
        var workplace = await _workplaceRepository.GetByIdAsync(workplaceId);
        if (workplace == null)
        {
            _logger.LogWarning("Workplace not found: {WorkplaceId}", workplaceId);
            return ServiceResult.NotFound("Workplace not found");
        }

        var limit = await _limitRepository.GetByIdAsync(limitId);
        if (limit == null || limit.WorkplaceId != workplaceId)
        {
            _logger.LogWarning("Limit not found: {LimitId}", limitId);
            return ServiceResult.NotFound("Limit not found");
        }

        if (dto.PeriodFrom >= dto.PeriodTo)
        {
            _logger.LogWarning("Invalid period dates: {From} >= {To}", dto.PeriodFrom, dto.PeriodTo);
            return ServiceResult.BadRequest("Period start date must be before end date");
        }

        var hasOverlap = await _limitRepository.HasOverlappingLimitAsync(
            workplaceId, dto.CategoryId, dto.PeriodFrom, dto.PeriodTo, limitId);

        if (hasOverlap)
        {
            _logger.LogWarning("Overlapping limit exists for category {CategoryId}", dto.CategoryId);
            return ServiceResult.BadRequest("A limit for this category already exists in the specified period");
        }

        limit.PeriodFrom = dto.PeriodFrom;
        limit.PeriodTo = dto.PeriodTo;
        limit.LimitAmount = dto.LimitAmount;
        limit.Currency = dto.Currency;
        limit.CategoryId = dto.CategoryId;
        limit.IsActive = dto.IsActive;

        _limitRepository.Update(limit);
        await _limitRepository.SaveChangesAsync();

        _logger.LogInformation("Limit updated successfully: {LimitId}", limitId);
        return ServiceResult.Success();
    }

    /// <summary>
    /// Deletes a spending limit from a workplace.
    /// </summary>
    /// <param name="workplaceId">The workplace ID.</param>
    /// <param name="limitId">The limit ID.</param>
    /// <returns>Success or failure result.</returns>
    public async Task<ServiceResult> DeleteLimitAsync(Guid workplaceId, Guid limitId)
    {
        _logger.LogInformation("Deleting limit {LimitId} from workplace {WorkplaceId}", limitId, workplaceId);
        var workplace = await _workplaceRepository.GetByIdAsync(workplaceId);
        if (workplace == null)
        {
            _logger.LogWarning("Workplace not found: {WorkplaceId}", workplaceId);
            return ServiceResult.NotFound("Workplace not found");
        }

        var limit = await _limitRepository.GetByIdAsync(limitId);
        if (limit == null || limit.WorkplaceId != workplaceId)
        {
            _logger.LogWarning("Limit not found: {LimitId}", limitId);
            return ServiceResult.NotFound("Limit not found");
        }

        _limitRepository.Remove(limit);
        await _limitRepository.SaveChangesAsync();

        _logger.LogInformation("Limit {LimitId} deleted from workplace {WorkplaceId}", limitId, workplaceId);
        return ServiceResult.Success();
    }

    /// <summary>
    /// Calculates the usage statistics for a specific limit including used and remaining amounts.
    /// </summary>
    /// <param name="workplaceId">The workplace ID.</param>
    /// <param name="limitId">The limit ID.</param>
    /// <returns>Limit usage statistics.</returns>
    public async Task<ServiceResult<LimitUsageDto>> GetLimitUsageAsync(Guid workplaceId, Guid limitId)
    {
        _logger.LogInformation("Getting usage for limit {LimitId} in workplace {WorkplaceId}", limitId, workplaceId);
        var workplace = await _workplaceRepository.GetByIdAsync(workplaceId);
        if (workplace == null)
        {
            _logger.LogWarning("Workplace not found: {WorkplaceId}", workplaceId);
            return ServiceResult<LimitUsageDto>.NotFound("Workplace not found");
        }

        var limit = await _limitRepository.GetByIdAsync(limitId);
        if (limit == null || limit.WorkplaceId != workplaceId)
        {
            _logger.LogWarning("Limit not found: {LimitId}", limitId);
            return ServiceResult<LimitUsageDto>.NotFound("Limit not found");
        }

        var usedAmount = await _limitRepository.GetUsedAmountAsync(limitId);
        var remainingAmount = limit.LimitAmount - usedAmount;

        return ServiceResult<LimitUsageDto>.Success(new LimitUsageDto
        {
            LimitId = limitId,
            LimitAmount = limit.LimitAmount,
            UsedAmount = usedAmount,
            RemainingAmount = remainingAmount > 0 ? remainingAmount : 0,
            Currency = limit.Currency,
            IsExceeded = usedAmount > limit.LimitAmount
        });
    }

    /// <summary>
    /// Maps a WorkplaceLimit entity to its DTO representation.
    /// </summary>
    private static WorkplaceLimitDto MapToDto(WorkplaceLimit limit) => new()
    {
        Id = limit.Id,
        WorkplaceId = limit.WorkplaceId,
        PeriodFrom = limit.PeriodFrom,
        PeriodTo = limit.PeriodTo,
        LimitAmount = limit.LimitAmount,
        Currency = limit.Currency,
        CategoryId = limit.CategoryId,
        IsActive = limit.IsActive,
        CreatedAt = limit.CreatedAt,
        CreatedBy = limit.CreatedBy
    };
}
