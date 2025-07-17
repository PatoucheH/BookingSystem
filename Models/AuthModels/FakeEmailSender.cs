using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

public class FakeEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        Console.WriteLine($"FAKE EMAIL SENT TO: {email}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"Message: {htmlMessage}");
        return Task.CompletedTask;
    }
}
