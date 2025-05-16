using MimeKit;
using MailKit.Net.Smtp;
using Truckero.Core.Interfaces;

namespace Truckero.Infrastructure.Services;

public class EmailService : IEmailService
{
    public async Task SendPasswordResetAsync(string toEmail, string resetLink)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Truckero Support", "no-reply@truckero.app"));
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
