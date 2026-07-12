namespace SufiChain.SufiPlatform.UI.Localization;

/// <summary>
/// Legacy UI localization options.
/// Configure application languages via <c>AbpLocalizationOptions</c> instead;
/// the default language provider reads that list as the single source of truth.
/// Kept for binary compatibility; prefer not to populate <see cref="Languages"/>.
/// </summary>
public class SufiLocalizationOptions
{
    /// <summary>
    /// Obsolete language list. Prefer configuring <c>AbpLocalizationOptions.Languages</c>.
    /// </summary>
    public List<LanguageInfo> Languages { get; } = new();
}
