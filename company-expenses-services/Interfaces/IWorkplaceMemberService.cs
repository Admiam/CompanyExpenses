using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;

namespace CompanyExpenses.Services.Interfaces;

/// <summary>
/// Workplace member business service interface
/// </summary>
public interface IWorkplaceMemberService
{
    Task<ServiceResult<IEnumerable<WorkplaceMemberDto>>> GetMembersByWorkplaceAsync(Guid workplaceId);
    Task<ServiceResult<WorkplaceMemberDto>> GetMemberByIdAsync(Guid workplaceId, Guid memberId);
    Task<ServiceResult<WorkplaceMemberDto>> AddMemberAsync(Guid workplaceId, CreateWorkplaceMemberDto dto, string userId);
    Task<ServiceResult> UpdateMemberAsync(Guid workplaceId, Guid memberId, UpdateWorkplaceMemberDto dto);
    Task<ServiceResult> RemoveMemberAsync(Guid workplaceId, Guid memberId);
    Task<ServiceResult<IEnumerable<WorkplaceDto>>> GetWorkplacesByUserAsync(string userId);
    Task<ServiceResult<bool>> IsUserManagerAsync(string userId, Guid workplaceId);
    Task<ServiceResult<IEnumerable<UserWithStatsDto>>> GetUsersWithStatsAsync(bool includeInactive);
    Task<ServiceResult<IEnumerable<UserWithStatsDto>>> GetInactiveUsersAsync();
    Task<ServiceResult<UserDetailDto>> GetUserDetailAsync(string userId);
}
