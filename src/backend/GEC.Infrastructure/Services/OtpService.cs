using System.Security.Cryptography;
using System.Text;
using GEC.ApplicationCore.Interfaces.Services;
using GEC.ApplicationCore.Options;
using Microsoft.Extensions.Options;

namespace GEC.Infrastructure.Services;

public class OtpService : IOtpService
{
    private readonly byte[] _pepperBytes;
    private readonly OtpOptions _options;

    public OtpService(IOptions<OtpOptions> options)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(_options.PepperKey))
        {
            throw new InvalidOperationException("OTP Pepper missing from configuration.");
        }

        _pepperBytes = Encoding.UTF8.GetBytes(_options.PepperKey);
    }

    public string GenerateOtp()
    {
        const string digits = "0123456789";
        return RandomNumberGenerator.GetString(digits, _options.Length);
    }

    public string HashOtp(string otp, string salt)
    {
        byte[] dataToHash = Encoding.UTF8.GetBytes($"{otp}:{salt}");
        byte[] hashBytes = HMACSHA256.HashData(_pepperBytes, dataToHash);
        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyOtpHash(string inputOtp, string salt, string expectedHash)
    {
        if (string.IsNullOrWhiteSpace(inputOtp) || string.IsNullOrWhiteSpace(expectedHash))
        {
            return false;
        }

        string actualHash = HashOtp(inputOtp, salt);

        byte[] expectedHashBytes = Convert.FromBase64String(expectedHash);
        byte[] actualHashBytes = Convert.FromBase64String(actualHash);

        return CryptographicOperations.FixedTimeEquals(expectedHashBytes, actualHashBytes);
    }
}