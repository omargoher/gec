using GEC.ApplicationCore.Exceptions;
using GEC.ApplicationCore.Interfaces.Services;
using GEC.ApplicationCore.Options;
using Microsoft.Extensions.Options;

namespace GEC.Infrastructure.Services;

public class OtpManager : IOtpManager
{
    private readonly IOtpService _otpService;
    private readonly ICacheService _cacheService;
    private readonly IEmailService _emailService;
    private readonly OtpOptions _otpOptions;

    public OtpManager(IOtpService otpService, ICacheService cacheService, IEmailService emailService, IOptions<OtpOptions> otpOptions)
    {
        _otpService = otpService;
        _cacheService = cacheService;
        _emailService = emailService;
        _otpOptions = otpOptions.Value;
    }

    public async Task SendOtpAsync(string recipient, string purpose, CancellationToken cancellationToken = default)
    {
        var otp = _otpService.GenerateOtp();
        string salt = BuildSalt(recipient, purpose);

        var hashedOtp = _otpService.HashOtp(otp, salt);

        var otpCacheEntry = new OtpCacheEntry
        {
            HashedCode = hashedOtp,
            Attempts = 0
        };

        await _cacheService.SetAsync(
            BuildCacheKey(recipient, purpose),
            otpCacheEntry,
            TimeSpan.FromMinutes(_otpOptions.ExpiryInMinutes),
            cancellationToken);

        string templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "OtpEmailTemplate.html");
        string htmlBody = await File.ReadAllTextAsync(templatePath, cancellationToken);
        htmlBody = htmlBody
            .Replace("{{HEADLINE}}", $"Your {purpose} code")
            .Replace("{{OTP_CODE}}", otp)
            .Replace("{{EXPIRY_MINUTES}}", _otpOptions.ExpiryInMinutes.ToString())
            .Replace("{{SUPPORT_EMAIL}}", "support@gec.com")
            .Replace("{{YEAR}}", DateTime.Now.Year.ToString());

        await _emailService.SendEmailAsync(
            recipient,
            $"Your {purpose} code: {otp}",
            htmlBody,
            cancellationToken);
    }

    public async Task ValidateOtpAsync(string otp, string recipient, string purpose, CancellationToken cancellationToken = default)
    {
        string salt = BuildSalt(recipient, purpose);
        string cacheKey = BuildCacheKey(recipient, purpose);

        var otpCacheEntry = await _cacheService.GetAsync<OtpCacheEntry>(cacheKey, cancellationToken);

        if (otpCacheEntry == null)
        {
            throw new InvalidOtpException("Invalid or expired OTP.");
        }

        if (!_otpService.VerifyOtpHash(otp, salt, otpCacheEntry.HashedCode))
        {
            otpCacheEntry.Attempts++;

            if (otpCacheEntry.Attempts >= _otpOptions.MaxAttempts)
            {
                await _cacheService.DeleteAsync(cacheKey, cancellationToken);
            }
            else
            {
                var elapsed = DateTimeOffset.UtcNow - otpCacheEntry.CreatedAt;
                var remainingLife = TimeSpan.FromMinutes(_otpOptions.ExpiryInMinutes) - elapsed;

                if (remainingLife > TimeSpan.Zero)
                {
                    await _cacheService.SetAsync(cacheKey, otpCacheEntry, remainingLife, cancellationToken);
                }
                else
                {
                    await _cacheService.DeleteAsync(cacheKey, cancellationToken);
                }
            }

            throw new InvalidOtpException("Invalid or expired OTP.");
        }

        await _cacheService.DeleteAsync(cacheKey, cancellationToken);
    }

    private static string BuildSalt(string recipient, string purpose)
        => $"{purpose}:{recipient.ToLowerInvariant()}";

    private static string BuildCacheKey(string recipient, string purpose)
        => $"otp:{purpose}:{recipient.ToLowerInvariant()}";

}

public class OtpCacheEntry
{
    public required string HashedCode { get; init; }
    public int Attempts { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}