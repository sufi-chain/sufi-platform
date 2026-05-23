using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiAbp.SettingManagement;

public class SendTestEmailInput
{
    [Required]
    [EmailAddress]
    public string SenderEmailAddress { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string TargetEmailAddress { get; set; } = string.Empty;
    
    [Required]
    public string Subject { get; set; } = string.Empty;
    
    public string? Body { get; set; }
}
