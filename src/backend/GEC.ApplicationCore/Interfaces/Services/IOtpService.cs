namespace GEC.ApplicationCore.Interfaces.Services;

public interface IOtpService
{
    string GenerateOtp();
    string HashOtp(string otp, string salt);
    bool VerifyOtpHash(string inputOtp, string salt, string expectedHash);
}