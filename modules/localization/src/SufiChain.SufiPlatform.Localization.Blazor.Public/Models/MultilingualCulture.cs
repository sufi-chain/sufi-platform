namespace SufiChain.SufiPlatform.Localization.Blazor.Public.Models;

public sealed class MultilingualCulture
{
    public string CultureName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public bool IsRtl { get; set; }

    public bool IsDefault { get; set; }
}
