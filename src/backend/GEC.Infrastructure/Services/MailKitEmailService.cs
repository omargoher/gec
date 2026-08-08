using GEC.ApplicationCore.Interfaces.Services;
using GEC.ApplicationCore.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace GEC.Infrastructure.Services;

public class MailKitEmailService : IEmailService
{
    private readonly MailOptions _options;

    public MailKitEmailService(IOptions<MailOptions> mailOptions)
    {
        _options = mailOptions.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlContent,
        CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
        message.To.Add(new MailboxAddress(null, to));
        message.Subject = subject;
        message.Body = new TextPart(TextFormat.Html)
        {
            Text = htmlContent
        };

        using var client = new SmtpClient();

        await client.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.Auto, cancellationToken);
        await client.AuthenticateAsync(_options.UserName, _options.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}