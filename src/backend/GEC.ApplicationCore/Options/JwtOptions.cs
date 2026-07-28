namespace GEC.ApplicationCore.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public required string Key { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}