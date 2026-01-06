using CompanyExpenses.Database.Repositories;
using CompanyExpenses.Models.Entities;
using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;
using CompanyExpenses.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompanyExpenses.Services.Implementations;

/// <summary>
/// Workplace limit business service implementation
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

    public async Task<ServiceResult<IEnumerable<WorkplaceLimitDto>>> GetLimitsByWorkplaceAsync(Guid workplaceId)
    {
        var workplace = await _workplaceRepository.GetByIdAsync(workplaceId);
        if (workplace == null)
            return ServiceResult<IEnumerable<WorkplaceLimitDto>>.NotFound("Workplace not found");

        var limits = await _limitRepository.GetByWorkplaceIdAsync(workplaceId);
        var result = limits.Select(MapToDto);

        return ServiceResult<IEnumerable<WorkplaceLimitDto>>.Success(result);
    }

    public async Task<ServiceResult<WorkplaceLimitDto>> GetLimitByIdAsync(Guid workplaceId, Guid limitId)
    {
        var workplace = await _workplaceRepository.GetByIdAsync(workplaceId);
        if (workplace == null)
            return ServiceResult<WorkplaceLimitDto>.NotFound("Workplace not found");

        var limit = await _limitRepository.GetByIdAsync(limitId);
        if (limit == null || limit.WorkplaceId != workplaceId)
            return ServiceResult<WorkplaceLimitDto>.NotFound("Limit not found");

        return ServiceResult<WorkplaceLimitDto>.Success(MapToDto(limit));
    }

    public async Task<ServiceResult<WorkplaceLimitDto>> CreateLimitAsync(Guid workplaceId, CreateWorkplaceLimitDto dto, string userId)
    {
        var workplace = await _workplaceRepository.GetByIdAsync(workplaceId);
        if (workplace == null)
            return ServiceResult<WorkplaceLimitDto>.NotFound("Workplace not found");

        // Validate period dates
        if (dto.PeriodFrom >= dto.PeriodTo)
            return ServiceResult<WorkplaceLimitDto>.BadRequest("Period start date must be before end date");

        // Check for overlapping limits
        var hasOverlap = await _limitRepository.HasOverlappingLimitAsync(
            workplaceId, dto.CategoryId, dto.PeriodFrom, dto.PeriodTo);

        if (hasOverlap)
            return ServiceResult<WorkplaceLimitDto>.BadRequest("A limit for this category already exists in the specified period");

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

    public async Task<ServiceResult> UpdateLimitAsync(Guid workplaceId, Guid limitId, UpdateWorkplaceLimitDto dto)
    {
        var workplace = await _workplaceRepository.GetByIdAsync(workplaceId);
        if (workplace == null)
            return ServiceResult.NotFound("Workplace not found");

        var limit = await _limitRepository.GetByIdAsync(limitId);
        if (limit == null || limit.WorkplaceId != workplaceId)
            return ServiceResult.NotFound("Limit not found");

        // Validate period dates
        if (dto.PeriodFrom >= dto.PeriodTo)
            return ServiceResult.BadRequest("Period start date must be before end date");

        // Check for overlapping limits (excluding this one)
        var hasOverlap = await _limitRepository.HasOverlappingLimitAsync(
            workplaceId, dto.CategoryId, dto.PeriodFrom, dto.PeriodTo, limitId);

        if (hasOverlap)
            return ServiceResult.BadRequest("A limit for this category already exists in the specified period");

        limit.PeriodFrom = dto.PeriodFrom;
        limit.PeriodTo = dto.PeriodTo;
        limit.LimitAmount = dto.LimitAmount;
        limit.Currency = dto.Currency;
        limit.CategoryId = dto.CategoryId;
        limit.IsActive = dto.IsActive;

        _limitRepository.Update(limit);
        await _limitRepository.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteLimitAsync(Guid workplaceId, Guid limitId)
    {
        var workplace = await _workplaceRepository.GetByIdAsync(workplaceId);
        if (workplace == null)
            return ServiceResult.NotFound("Workplace not found");

        var limit = await _limitRepository.GetByIdAsync(limitId);
        if (limit == null || limit.WorkplaceId != workplaceId)
            return ServiceResult.NotFound("Limit not found");

        _limitRepository.Remove(limit);
        await _limitRepository.SaveChangesAsync();

        _logger.LogInformation("Limit {LimitId} deleted from workplace {WorkplaceId}", limitId, workplaceId);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult<LimitUsageDto>> GetLimitUsageAsync(Guid workplaceId, Guid limitId)
    {
        var workplace = await _workplaceRepository.GetByIdAsync(workplaceId);
        if (workplace == null)
            return ServiceResult<LimitUsageDto>.NotFound("Workplace not found");

        var limit = await _limitRepository.GetByIdAsync(limitId);
        if (limit == null || limit.WorkplaceId != workplaceId)
            return ServiceResult<LimitUsageDto>.NotFound("Limit not found");

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
