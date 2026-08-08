namespace GEC.ApplicationCore.Options;

public class ExternalAuthOptions
{
    public const string SectionName = "ExternalAuthSettings";
    public required string GoogleClientId { get; set; }
}