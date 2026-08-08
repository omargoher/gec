namespace GEC.ApplicationCore.Interfaces.Services;

public interface IOtpManager
{
    Task SendOtpAsync(string recipient, string purpose, CancellationToken cancellationToken = default);
    Task ValidateOtpAsync(string otp, string recipient, string purpose, CancellationToken cancellationToken = default);
}