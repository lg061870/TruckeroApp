using Truckero.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Truckero.Diagnostics.Mocks;

/// <summary>
/// Simulates sending email for development/testing. Logs instead of actually sending.
/// </summary>
public class DevEmailService : IEmailService
{
    private readonly ILogger<DevEmailService> _logger;

    public DevEmailService(ILogger<DevEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendPasswordResetAsync(string toEmail, string resetLink)
    {
        _logger.LogInformation("[DEV] Simulating password reset email:");
        _logger.LogInformation("To: {Email}", toEmail);
        _logger.LogInformation("Reset Link: {ResetLink}", resetLink);
        return Task.CompletedTask;
    }
}
