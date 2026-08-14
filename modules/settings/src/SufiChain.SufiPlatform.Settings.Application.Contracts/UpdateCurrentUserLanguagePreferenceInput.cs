using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.Settings;

public class UpdateCurrentUserLanguagePreferenceInput
{
    [Required]
    public string CultureName { get; set; } = string.Empty;
}
