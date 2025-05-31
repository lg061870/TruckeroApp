using Truckero.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Truckero.Diagnostics.Mocks;

/// <summary>
/// Simulates sending email for development/testing. Logs instead of actually sending.
/// </summary>
public class DevEmailService : IEmailService
{
    private readonly ILogger<DevEmailService> _logger;
    private static readonly string ConfirmationFolder = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Truckero.API", "EmailConfirmation");
    private const string LocalApiBaseUrl = "https://localhost:5001/onboarding/confirm-email?token=";

    public DevEmailService(ILogger<DevEmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendPasswordResetAsync(string toEmail, string resetLink)
    {
        _logger.LogInformation("[DEV] Simulating password reset email:");
        _logger.LogInformation("To: {Email}", toEmail);
        _logger.LogInformation("Reset Link: {ResetLink}", resetLink);

        Directory.CreateDirectory(ConfirmationFolder);
        var html = $@"
        <html>
        <head><title>Truckero Password Reset</title></head>
        <body style='font-family:sans-serif;'>
            <h2>Password Reset Requested</h2>
            <p>Hi <b>{toEmail}</b>,</p>
            <p>We received a request to reset your password.</p>
            <p>You can reset it by clicking the link below:</p>
            <p><a href='{resetLink}'>{resetLink}</a></p>
            <p>If you did not request this, you can safely ignore this email.</p>
            <br/>
            <p>Thank you,<br/>The Truckero Team</p>
        </body>
        </html>";

        var fileName = $"reset-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString().Substring(0, 8)}.html";
        var filePath = Path.Combine(ConfirmationFolder, fileName);
        await File.WriteAllTextAsync(filePath, html);

        _logger.LogInformation("[DEV] Password reset email saved to: {FilePath}", filePath);
    }


    public async Task SendConfirmationAsync(string toEmail, string confirmationToken)
    {
        _logger.LogInformation("[DEV] Simulating confirmation email:");
        _logger.LogInformation("To: {Email}", toEmail);
        _logger.LogInformation("Confirmation Token: {Token}", confirmationToken);

        Directory.CreateDirectory(ConfirmationFolder);
        var confirmationLink = $"{LocalApiBaseUrl}{confirmationToken}";
        var html = $@"
            <html>
            <head><title>Truckero Email Confirmation</title></head>
            <body style='font-family:sans-serif;'>
                <h2>Welcome to Truckero!</h2>
                <p>Hi <b>{toEmail}</b>,</p>
                <p>To complete your registration, please confirm your email address by clicking the link below:</p>
                <p><a href='{confirmationLink}'>{confirmationLink}</a></p>
                <p>If you did not sign up for Truckero, you can safely ignore this email.</p>
                <br/>
                <p>Thank you,<br/>The Truckero Team</p>
            </body>
            </html>";
        var fileName = $"confirmation-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString().Substring(0, 8)}.html";
        var filePath = Path.Combine(ConfirmationFolder, fileName);
        await File.WriteAllTextAsync(filePath, html);
        _logger.LogInformation("[DEV] Confirmation email saved to: {FilePath}", filePath);
    }
}
