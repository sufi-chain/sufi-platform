namespace SufiChain.SufiPlatform.Settings;

public class EmailSettingsDto
{
    /// <summary>
    /// A tenant may see that email is host-locked. The payload then carries no SMTP secrets.
    /// </summary>
    public bool LockedByHost { get; set; }

    public string? DefaultFromAddress { get; set; }
    public string? DefaultFromDisplayName { get; set; }
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public bool SmtpEnableSsl { get; set; }
    public bool SmtpUseDefaultCredentials { get; set; }
    public string? SmtpUserName { get; set; }
    public string? SmtpPassword { get; set; }
    public string? SmtpDomain { get; set; }
}
