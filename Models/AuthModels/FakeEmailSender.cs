using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

/// <summary>
/// A fake implementation of the <see cref="IEmailSender"/> interface for testing purposes.
/// </summary>
public class FakeEmailSender : IEmailSender
{
    /// <summary>
    /// Sends an email asynchronously with the specified recipient, subject, and HTML message content.
    /// </summary>
    /// <param name="email">The email address of the recipient. Cannot be null or empty.</param>
    /// <param name="subject">The subject of the email. Cannot be null or empty.</param>
    /// <param name="htmlMessage">The HTML content of the email message. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        Console.WriteLine($"FAKE EMAIL SENT TO: {email}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"Message: {htmlMessage}");
        return Task.CompletedTask;
    }
}
