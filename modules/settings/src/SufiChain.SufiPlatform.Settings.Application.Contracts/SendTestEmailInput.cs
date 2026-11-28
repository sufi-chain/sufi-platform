using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.Settings;

public class SendTestEmailInput
{
    [Required]
    [EmailAddress]
    [Display(Name = "Field:SenderEmailAddress")]
    public string SenderEmailAddress { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [Display(Name = "Field:TargetEmailAddress")]
    public string TargetEmailAddress { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Field:Subject")]
    public string Subject { get; set; } = string.Empty;
    
    public string? Body { get; set; }
}
