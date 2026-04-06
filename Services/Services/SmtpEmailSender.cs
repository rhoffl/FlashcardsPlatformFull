using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace FlashcardsPlatformFull.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;

    public SmtpEmailSender(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var host = _config["Email:Smtp:Host"];
        var port = int.Parse(_config["Email:Smtp:Port"] ?? "587");
        var username = _config["Email:Smtp:Username"];
        var password = _config["Email:Smtp:Password"];
        var from = _config["Email:Smtp:From"];

        var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true
        };

        var mail = new MailMessage(from!, email, subject, htmlMessage)
        {
            IsBodyHtml = true
        };

        await client.SendMailAsync(mail);
    }
}