using System.Net;
using System.Net.Mail;

namespace CompanyExpenses.Api.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendInvitationEmailAsync(string email, string token, string? workplaceName)
    {
        var authServerUrl = _configuration["AppSettings:AuthServerUrl"] ?? "https://localhost:7169";
        var invitationLink = $"{authServerUrl}/Account/Register?token={token}";

        var subject = "You're invited to Company Expenses!";
        var htmlMessage = $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #2563eb;'>Welcome to Company Expenses!</h2>
                    <p>Hi,</p>
                    <p>You've been invited to join Company Expenses{(workplaceName != null ? $" for workplace <strong>{workplaceName}</strong>" : "")}.</p>
                    <p>Click the button below to complete your registration:</p>
                    <div style='margin: 30px 0; text-align: center;'>
                        <a href='{invitationLink}' 
                           style='background-color: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 8px; display: inline-block; font-weight: bold;'>
                            Complete Registration
                        </a>
                    </div>
                    <p>Or copy and paste this link into your browser:</p>
                    <p style='word-break: break-all; color: #2563eb;'>{invitationLink}</p>
                    <p style='margin-top: 20px;'><strong>Note:</strong> This invitation will expire in 7 days.</p>
                    <hr style='margin: 30px 0; border: none; border-top: 1px solid #e5e7eb;'>
                    <p style='font-size: 12px; color: #6b7280;'>
                        If you didn't expect this invitation, please ignore this email.
                    </p>
                </div>
            </body>
            </html>";

        await SendEmailAsync(email, subject, htmlMessage);
    }

    private async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            var smtpHost = _configuration["EmailSettings:SmtpHost"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            var fromEmail = _configuration["EmailSettings:FromEmail"];
            var fromName = _configuration["EmailSettings:FromName"] ?? "Company Expenses";

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername) ||
                string.IsNullOrEmpty(smtpPassword) || string.IsNullOrEmpty(fromEmail))
            {
                _logger.LogWarning("Email settings not configured. Email not sent to {Email}", email);
                return;
            }

            using var smtpClient = new SmtpClient(smtpHost)
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Invitation email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending invitation email to {Email}", email);
            // Don't throw - we don't want to fail the invitation creation if email fails
        }
    }
}
