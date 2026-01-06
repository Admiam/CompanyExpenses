namespace CompanyExpenses.Services.Interfaces;

/// <summary>
/// Email service interface for sending notifications
/// </summary>
public interface IEmailService
{
    Task SendInvitationEmailAsync(string toEmail, string invitationToken, string? workplaceName = null);
    Task SendExpenseApprovalEmailAsync(string toEmail, string expenseTitle, bool approved, string? note = null);
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
    Task SendWelcomeEmailAsync(string toEmail, string userName);
}
