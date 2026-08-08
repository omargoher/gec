namespace GEC.ApplicationCore.Options;

public class MailOptions
{
    public const string SectionName = "MailSettings";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string FromName { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}