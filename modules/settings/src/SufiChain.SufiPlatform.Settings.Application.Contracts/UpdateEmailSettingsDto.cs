using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.Settings;

public class UpdateEmailSettingsDto
{
    [EmailAddress]
    public string? DefaultFromAddress { get; set; }
    
    public string? DefaultFromDisplayName { get; set; }
    
    [Required]
    public string SmtpHost { get; set; } = string.Empty;
    
    [Range(1, 65535)]
    public int SmtpPort { get; set; } = 587;
    
    public bool SmtpEnableSsl { get; set; } = true;
    
    public bool SmtpUseDefaultCredentials { get; set; }
    
    public string? SmtpUserName { get; set; }
    
    public string? SmtpPassword { get; set; }
    
    public string? SmtpDomain { get; set; }
}
