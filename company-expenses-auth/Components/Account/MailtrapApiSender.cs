using company_expenses_auth.Data;
using Microsoft.AspNetCore.Identity;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace company_expenses_auth.Components.Account
{
    /// <summary>
    /// Mailtrap API Email Sender - For production email sending
    /// Uses Mailtrap Send API instead of SMTP
    /// </summary>
    public class MailtrapApiSender : IEmailSender<ApplicationUser>
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MailtrapApiSender> _logger;
        private readonly HttpClient _httpClient;

        public MailtrapApiSender(IConfiguration configuration, ILogger<MailtrapApiSender> logger, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {
            var subject = "Confirm your email - Company Expenses";
            var htmlBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #2563eb;'>Welcome to Company Expenses!</h2>
                        <p>Hi {user.Email},</p>
                        <p>Thank you for registering. Please confirm your email address by clicking the button below:</p>
                        <div style='margin: 30px 0; text-align: center;'>
                            <a href='{confirmationLink}' 
                               style='background-color: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 8px; display: inline-block; font-weight: bold;'>
                                Confirm Email Address
                            </a>
                        </div>
                        <p>Or copy and paste this link into your browser:</p>
                        <p style='word-break: break-all; color: #2563eb;'>{confirmationLink}</p>
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #e5e7eb;'>
                        <p style='font-size: 12px; color: #6b7280;'>
                            If you didn't create an account, please ignore this email.
                        </p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, htmlBody);
        }

        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            var subject = "Reset your password - Company Expenses";
            var htmlBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #2563eb;'>Password Reset Request</h2>
                        <p>Hi {user.Email},</p>
                        <p>You requested to reset your password. Click the button below to continue:</p>
                        <div style='margin: 30px 0; text-align: center;'>
                            <a href='{resetLink}' 
                               style='background-color: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 8px; display: inline-block; font-weight: bold;'>
                                Reset Password
                            </a>
                        </div>
                        <p>Or copy and paste this link into your browser:</p>
                        <p style='word-break: break-all; color: #2563eb;'>{resetLink}</p>
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #e5e7eb;'>
                        <p style='font-size: 12px; color: #6b7280;'>
                            If you didn't request a password reset, please ignore this email.
                        </p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, htmlBody);
        }

        public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            var subject = "Password Reset Code - Company Expenses";
            var htmlBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #2563eb;'>Password Reset Code</h2>
                        <p>Hi {user.Email},</p>
                        <p>Your password reset code is:</p>
                        <div style='margin: 30px 0; text-align: center;'>
                            <div style='background-color: #f3f4f6; padding: 20px; border-radius: 8px; font-size: 24px; font-weight: bold; letter-spacing: 4px;'>
                                {resetCode}
                            </div>
                        </div>
                        <p>Enter this code in the application to reset your password.</p>
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #e5e7eb;'>
                        <p style='font-size: 12px; color: #6b7280;'>
                            This code will expire in 1 hour.
                        </p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, htmlBody);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var apiToken = _configuration["EmailSettings:MailtrapApiToken"];
                var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@holu.be";
                var fromName = _configuration["EmailSettings:FromName"] ?? "Company Expenses";

                if (string.IsNullOrEmpty(apiToken))
                {
                    throw new InvalidOperationException("Mailtrap API token is not configured");
                }

                var payload = new
                {
                    from = new { email = fromEmail, name = fromName },
                    to = new[] { new { email = toEmail } },
                    subject = subject,
                    html = htmlBody,
                    category = "Authentication"
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "https://send.api.mailtrap.io/api/send");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
                request.Content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email sent successfully to {Email} via Mailtrap API", toEmail);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to send email. Status: {Status}, Error: {Error}",
                        response.StatusCode, errorContent);
                    throw new InvalidOperationException($"Failed to send email: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email} via Mailtrap API", toEmail);
                throw;
            }
        }
    }
}
