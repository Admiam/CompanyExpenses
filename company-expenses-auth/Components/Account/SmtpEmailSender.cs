using company_expenses_auth.Data;
using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Net.Mail;

namespace company_expenses_auth.Components.Account
{
    public class SmtpEmailSender : IEmailSender<ApplicationUser>
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {
            var subject = "Confirm your email - Company Expenses";
            var htmlMessage = $@"
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

            await SendEmailAsync(email, subject, htmlMessage);
        }

        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            var subject = "Reset your password - Company Expenses";
            var htmlMessage = $@"
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
                            If you didn't request a password reset, please ignore this email. Your password will remain unchanged.
                        </p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, htmlMessage);
        }

        public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            var subject = "Password Reset Code - Company Expenses";
            var htmlMessage = $@"
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
                            This code will expire in 1 hour. If you didn't request this code, please ignore this email.
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

                using var smtpClient = new SmtpClient(smtpHost)
                {
                    Port = smtpPort,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail!, fromName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(email);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", email);
                throw;
            }
        }
    }
}
