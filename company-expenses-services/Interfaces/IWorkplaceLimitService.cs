using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;

namespace CompanyExpenses.Services.Interfaces;

/// <summary>
/// Workplace limit business service interface
/// </summary>
public interface IWorkplaceLimitService
{
    Task<ServiceResult<IEnumerable<WorkplaceLimitDto>>> GetLimitsByWorkplaceAsync(Guid workplaceId);
    Task<ServiceResult<WorkplaceLimitDto>> GetLimitByIdAsync(Guid workplaceId, Guid limitId);
    Task<ServiceResult<WorkplaceLimitDto>> CreateLimitAsync(Guid workplaceId, CreateWorkplaceLimitDto dto, string userId);
    Task<ServiceResult> UpdateLimitAsync(Guid workplaceId, Guid limitId, UpdateWorkplaceLimitDto dto);
    Task<ServiceResult> DeleteLimitAsync(Guid workplaceId, Guid limitId);
    Task<ServiceResult<LimitUsageDto>> GetLimitUsageAsync(Guid workplaceId, Guid limitId);
}
