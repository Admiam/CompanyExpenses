using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;

namespace CompanyExpenses.Services.Interfaces;

/// <summary>
/// Invitation business service interface
/// </summary>
public interface IInvitationService
{
    Task<ServiceResult<IEnumerable<InvitationDto>>> GetAllInvitationsAsync();
    Task<ServiceResult<InvitationDto>> GetInvitationByIdAsync(Guid id);
    Task<ServiceResult<InvitationDto>> VerifyInvitationAsync(string token);
    Task<ServiceResult<InvitationDto>> CreateInvitationAsync(CreateInvitationDto dto, string userId);
    Task<ServiceResult> AcceptInvitationAsync(Guid id, string userId);
    Task<ServiceResult> CancelInvitationAsync(Guid id);
    Task<ServiceResult> DeleteInvitationAsync(Guid id);
    Task<ServiceResult<InvitationDto>> ResendInvitationAsync(Guid id);
}
