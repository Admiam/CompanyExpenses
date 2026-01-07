using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;
using CompanyExpenses.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CompanyExpenses.Api.Controllers;

/// <summary>
/// Controller for invitation management operations including create, verify, accept, and resend.
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
    /// Retrieves all invitations in the system. Typically used by administrators.
    /// </summary>
    /// <returns>A list of all invitations.</returns>
    [HttpGet]
    public async Task<ActionResult> GetInvitations()
    {
        _logger.LogInformation("Fetching all invitations");
        var result = await _invitationService.GetAllInvitationsAsync();
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves a single invitation by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the invitation.</param>
    /// <returns>The invitation details if found, otherwise NotFound.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetInvitation(Guid id)
    {
        _logger.LogInformation("Fetching invitation with ID: {InvitationId}", id);
        var result = await _invitationService.GetInvitationByIdAsync(id);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Invitation not found with ID: {InvitationId}", id);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Verifies an invitation using its token. Used during the registration process.
    /// </summary>
    /// <param name="token">The unique invitation token.</param>
    /// <returns>The invitation details if valid, or error if expired/used.</returns>
    [HttpGet("verify/{token}")]
    public async Task<ActionResult> VerifyInvitation(string token)
    {
        _logger.LogInformation("Verifying invitation token");
        var result = await _invitationService.VerifyInvitationAsync(token);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Invitation verification failed: {ErrorMessage}", result.ErrorMessage);
        }
        else
        {
            _logger.LogInformation("Invitation token verified successfully");
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Creates a new invitation and sends an email to the invitee.
    /// </summary>
    /// <param name="dto">The invitation creation data transfer object.</param>
    /// <returns>The created invitation with its ID, or an error response.</returns>
    [HttpPost]
    public async Task<ActionResult> CreateInvitation([FromBody] CreateInvitationDto dto)
    {
        var userId = GetCurrentUserId() ?? "system";
        _logger.LogInformation("Creating invitation for email '{Email}' by user {UserId}", dto.Email, userId);

        var result = await _invitationService.CreateInvitationAsync(dto, userId);

        if (result.IsSuccess && result.Data != null)
        {
            _logger.LogInformation("Invitation created successfully with ID: {InvitationId}", result.Data.Id);
            return CreatedAtAction(nameof(GetInvitation), new { id = result.Data.Id }, result.Data);
        }

        _logger.LogWarning("Failed to create invitation: {ErrorMessage}", result.ErrorMessage);
        return HandleResult(result);
    }

    /// <summary>
    /// Accepts an invitation and adds the user to the associated workplace.
    /// </summary>
    /// <param name="id">The unique identifier of the invitation to accept.</param>
    /// <returns>Success message or error response.</returns>
    [HttpPost("{id}/accept")]
    public async Task<IActionResult> AcceptInvitation(Guid id)
    {
        var userId = GetCurrentUserId() ?? string.Empty;
        _logger.LogInformation("User {UserId} accepting invitation {InvitationId}", userId, id);

        var result = await _invitationService.AcceptInvitationAsync(id, userId);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Invitation {InvitationId} accepted successfully", id);
        }
        else
        {
            _logger.LogWarning("Failed to accept invitation {InvitationId}: {ErrorMessage}", id, result.ErrorMessage);
        }

        return HandleResult(result, "Invitation accepted successfully");
    }

    /// <summary>
    /// Cancels a pending invitation.
    /// </summary>
    /// <param name="id">The unique identifier of the invitation to cancel.</param>
    /// <returns>Success message or error response.</returns>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelInvitation(Guid id)
    {
        _logger.LogInformation("Cancelling invitation {InvitationId}", id);
        var result = await _invitationService.CancelInvitationAsync(id);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Invitation {InvitationId} cancelled successfully", id);
        }
        else
        {
            _logger.LogWarning("Failed to cancel invitation {InvitationId}: {ErrorMessage}", id, result.ErrorMessage);
        }

        return HandleResult(result, "Invitation cancelled successfully");
    }

    /// <summary>
    /// Permanently deletes an invitation from the system.
    /// </summary>
    /// <param name="id">The unique identifier of the invitation to delete.</param>
    /// <returns>NoContent on success, or error response.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInvitation(Guid id)
    {
        _logger.LogInformation("Deleting invitation {InvitationId}", id);
        var result = await _invitationService.DeleteInvitationAsync(id);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Invitation {InvitationId} deleted successfully", id);
            return NoContent();
        }

        _logger.LogWarning("Failed to delete invitation {InvitationId}: {ErrorMessage}", id, result.ErrorMessage);
        return HandleResult(result);
    }

    /// <summary>
    /// Resends an invitation email with a refreshed token and expiration date.
    /// </summary>
    /// <param name="id">The unique identifier of the invitation to resend.</param>
    /// <returns>The updated invitation or error response.</returns>
    [HttpPost("{id}/resend")]
    public async Task<ActionResult> ResendInvitation(Guid id)
    {
        _logger.LogInformation("Resending invitation {InvitationId}", id);
        var result = await _invitationService.ResendInvitationAsync(id);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Invitation {InvitationId} resent successfully", id);
        }
        else
        {
            _logger.LogWarning("Failed to resend invitation {InvitationId}: {ErrorMessage}", id, result.ErrorMessage);
        }

        return HandleResult(result);
    }

    #region Helper Methods

    /// <summary>
    /// Gets the current authenticated user's ID from the claims.
    /// </summary>
    /// <returns>The user ID if authenticated, otherwise null.</returns>
    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    /// <summary>
    /// Handles service result and returns appropriate HTTP response for generic results.
    /// </summary>
    /// <typeparam name="T">The type of data in the result.</typeparam>
    /// <param name="result">The service result to handle.</param>
    /// <returns>Appropriate HTTP response based on result status.</returns>
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

    /// <summary>
    /// Handles service result and returns appropriate HTTP response with optional success message.
    /// </summary>
    /// <param name="result">The service result to handle.</param>
    /// <param name="successMessage">Optional message to include on success.</param>
    /// <returns>Appropriate HTTP response based on result status.</returns>
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
