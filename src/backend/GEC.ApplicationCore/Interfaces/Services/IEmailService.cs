namespace GEC.ApplicationCore.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlContent,
        CancellationToken cancellationToken = default);
}