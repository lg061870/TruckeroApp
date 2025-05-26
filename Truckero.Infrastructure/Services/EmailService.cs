using MimeKit;
using MailKit.Net.Smtp;
using Truckero.Core.Interfaces;

namespace Truckero.Infrastructure.Services;

public class EmailService : IEmailService
{
    public async Task SendConfirmationAsync(string toEmail, string confirmationToken)
    {
        var confirmationLink = $"https://app.truckero.com/confirm-email?token={confirmationToken}";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Truckero Support", "lg061870@gmail.com"));
        message.To.Add(new MailboxAddress(toEmail, toEmail));
        message.Subject = "Confirm your Truckero account";
        message.Body = new TextPart("html")
        {
            Text = $@"
                <p>Welcome to Truckero!</p>
                <p>To complete your registration, please confirm your email address by clicking the link below:</p>
                <p><a href=""{confirmationLink}"">Confirm Email</a></p>
                <p>If you did not sign up for Truckero, you can safely ignore this email.</p>
                <br/>
                <p>Thank you,<br/>The Truckero Team</p>"
        };

        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.yourserver.com", 587, false);
        await client.AuthenticateAsync("youruser", "yourpassword");
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task SendPasswordResetAsync(string toEmail, string resetLink)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Truckero Support", "lg061870@gmail.com"));
        message.To.Add(new MailboxAddress(toEmail, toEmail));
        message.Subject = "Reset your Truckero password";
        message.Body = new TextPart("html")
        {
            Text = $"Click <a href=\"{resetLink}\">here</a> to reset your password."
        };

        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.yourserver.com", 587, false);
        await client.AuthenticateAsync("youruser", "yourpassword");
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
