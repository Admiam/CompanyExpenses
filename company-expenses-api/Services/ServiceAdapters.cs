using ServiceEmailInterface = CompanyExpenses.Services.Interfaces.IEmailService;
using ServiceImageInterface = CompanyExpenses.Services.Interfaces.IImageCompressionService;
using ApiEmailService = CompanyExpenses.Api.Services.IEmailService;
using ApiImageService = CompanyExpenses.Api.Services.IImageCompressionService;

namespace CompanyExpenses.Api.Services;

/// <summary>
/// Adapter to bridge API's ImageCompressionService to Service layer interface
/// </summary>
public class ImageCompressionServiceAdapter : ServiceImageInterface
{
    private readonly ApiImageService _apiService;

    public ImageCompressionServiceAdapter(ApiImageService apiService)
    {
        _apiService = apiService;
    }

    public async Task<(string base64Data, long compressedSize)> CompressImageToBase64Async(string base64Input, string contentType)
    {
        return await _apiService.CompressImageToBase64Async(base64Input, contentType);
    }
}

/// <summary>
/// Adapter to bridge API's EmailService to Service layer interface
/// </summary>
public class EmailServiceAdapter : ServiceEmailInterface
{
    private readonly ApiEmailService _apiService;

    public EmailServiceAdapter(ApiEmailService apiService)
    {
        _apiService = apiService;
    }

    public async Task SendInvitationEmailAsync(string toEmail, string invitationToken, string? workplaceName = null)
    {
        await _apiService.SendInvitationEmailAsync(toEmail, invitationToken, workplaceName);
    }

    public Task SendExpenseApprovalEmailAsync(string toEmail, string expenseTitle, bool approved, string? note = null)
    {
        // Not implemented in current API service
        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
    {
        // Not implemented in current API service
        return Task.CompletedTask;
    }

    public Task SendWelcomeEmailAsync(string toEmail, string userName)
    {
        // Not implemented in current API service
        return Task.CompletedTask;
    }
}
