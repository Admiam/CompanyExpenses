using CompanyExpenses.Models.Entities;
using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;

namespace CompanyExpenses.Services.Interfaces;

/// <summary>
/// Workplace business service interface
/// </summary>
public interface IWorkplaceService
{
    Task<ServiceResult<IEnumerable<WorkplaceDto>>> GetAllWorkplacesAsync();
    Task<ServiceResult<WorkplaceDetailDto>> GetWorkplaceByIdAsync(Guid id);
    Task<ServiceResult<WorkplaceDto>> CreateWorkplaceAsync(CreateWorkplaceDto dto, string userId);
    Task<ServiceResult> UpdateWorkplaceAsync(Guid id, UpdateWorkplaceDto dto);
    Task<ServiceResult<WorkplaceDependenciesDto>> GetDependenciesAsync(Guid id);
    Task<ServiceResult> DeleteWorkplaceAsync(Guid id);
}
