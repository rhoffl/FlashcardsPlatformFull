using Microsoft.AspNetCore.Identity.UI.Services;

namespace FlashcardsPlatformFull.Services;

public class ConsoleEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        Console.WriteLine("=== EMAIL (Console) ===");
        Console.WriteLine($"To: {email}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine(htmlMessage);
        Console.WriteLine("=======================");

        return Task.CompletedTask;
    }
}