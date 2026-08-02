namespace GEC.ApplicationCore.Options;

public class OtpOptions
{
    public const string SectionName = "OtpSettings";

    public string PepperKey { get; set; } = string.Empty;
    public int Length { get; set; } = 6;
    public int ExpiryInMinutes { get; set; } = 5;
    public int MaxAttempts { get; set; } = 3;
}