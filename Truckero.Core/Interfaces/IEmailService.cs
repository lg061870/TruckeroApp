namespace Truckero.Core.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetAsync(string toEmail, string resetLink);
    Task SendConfirmationAsync(string toEmail, string confirmationToken); // Add this
}