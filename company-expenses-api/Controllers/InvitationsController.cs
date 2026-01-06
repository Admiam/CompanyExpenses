using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;
using CompanyExpenses.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CompanyExpenses.Api.Controllers;

/// <summary>
/// Controller for invitation management - refactored to use Service layer
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class InvitationsController : ControllerBase
{
    private readonly IInvitationService _invitationService;
    private readonly ILogger<InvitationsController> _logger;

    public InvitationsController(
        IInvitationService invitationService,
        ILogger<InvitationsController> logger)
    {
        _invitationService = invitationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all invitations (for admin)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetInvitations()
    {
        var result = await _invitationService.GetAllInvitationsAsync();
        return HandleResult(result);
    }

    /// <summary>
    /// Get invitation by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetInvitation(Guid id)
    {
        var result = await _invitationService.GetInvitationByIdAsync(id);
        return HandleResult(result);
    }

    /// <summary>
    /// Verify invitation by token (used during registration)
    /// </summary>
    [HttpGet("verify/{token}")]
    public async Task<ActionResult> VerifyInvitation(string token)
    {
        var result = await _invitationService.VerifyInvitationAsync(token);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new invitation
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> CreateInvitation([FromBody] CreateInvitationDto dto)
    {
        var userId = GetCurrentUserId() ?? "system";
        var result = await _invitationService.CreateInvitationAsync(dto, userId);

        if (result.IsSuccess && result.Data != null)
        {
            return CreatedAtAction(nameof(GetInvitation), new { id = result.Data.Id }, result.Data);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Accept invitation
    /// </summary>
    [HttpPost("{id}/accept")]
    public async Task<IActionResult> AcceptInvitation(Guid id)
    {
        var userId = GetCurrentUserId() ?? string.Empty;
        var result = await _invitationService.AcceptInvitationAsync(id, userId);
        return HandleResult(result, "Invitation accepted successfully");
    }

    /// <summary>
    /// Cancel invitation
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelInvitation(Guid id)
    {
        var result = await _invitationService.CancelInvitationAsync(id);
        return HandleResult(result, "Invitation cancelled successfully");
    }

    /// <summary>
    /// Delete invitation
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInvitation(Guid id)
    {
        var result = await _invitationService.DeleteInvitationAsync(id);
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return HandleResult(result);
    }

    /// <summary>
    /// Resend invitation email
    /// </summary>
    [HttpPost("{id}/resend")]
    public async Task<ActionResult> ResendInvitation(Guid id)
    {
        var result = await _invitationService.ResendInvitationAsync(id);
        return HandleResult(result);
    }

    #region Helper Methods

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private ActionResult HandleResult<T>(ServiceResult<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(new { message = result.ErrorMessage }),
            ServiceErrorType.BadRequest => BadRequest(new { message = result.ErrorMessage }),
            ServiceErrorType.Unauthorized => Unauthorized(new { message = result.ErrorMessage }),
            _ => StatusCode(500, new { message = result.ErrorMessage })
        };
    }

    private ActionResult HandleResult(ServiceResult result, string? successMessage = null)
    {
        if (result.IsSuccess)
        {
            return Ok(new { message = successMessage ?? "Operation completed successfully" });
        }

        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(new { message = result.ErrorMessage }),
            ServiceErrorType.BadRequest => BadRequest(new { message = result.ErrorMessage }),
            ServiceErrorType.Unauthorized => Unauthorized(new { message = result.ErrorMessage }),
            _ => StatusCode(500, new { message = result.ErrorMessage })
        };
    }

    #endregion
}
