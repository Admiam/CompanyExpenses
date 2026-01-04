namespace CompanyExpenses.Api.Services;

public interface IEmailService
{
    Task SendInvitationEmailAsync(string email, string token, string? workplaceName);
}
