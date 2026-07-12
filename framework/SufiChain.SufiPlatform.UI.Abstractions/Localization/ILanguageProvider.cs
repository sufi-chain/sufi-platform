namespace SufiChain.SufiPlatform.UI.Localization;

/// <summary>
/// Provides available languages for the application.
/// </summary>
public interface ILanguageProvider
{
    /// <summary>
    /// Gets the list of available languages.
    /// </summary>
    Task<IReadOnlyList<LanguageInfo>> GetLanguagesAsync();
}

/// <summary>
/// Information about a language.
/// </summary>
public class LanguageInfo
{
    /// <summary>
    /// The culture name (e.g., "en-US").
    /// </summary>
    public string CultureName { get; set; } = string.Empty;

    /// <summary>
    /// The UI culture name (e.g., "en").
    /// </summary>
    public string UiCultureName { get; set; } = string.Empty;

    /// <summary>
    /// The display name of the language.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// The flag icon identifier (optional).
    /// </summary>
    public string? FlagIcon { get; set; }

    /// <summary>
    /// Whether this is a right-to-left language.
    /// </summary>
    public bool IsRtl { get; set; }

    /// <summary>
    /// Creates a new LanguageInfo.
    /// </summary>
    public LanguageInfo() { }

    /// <summary>
    /// Creates a new LanguageInfo with the specified values.
    /// </summary>
    public LanguageInfo(string cultureName, string uiCultureName, string displayName, bool isRtl = false, string? flagIcon = null)
    {
        CultureName = cultureName;
        UiCultureName = uiCultureName;
        DisplayName = displayName;
        IsRtl = isRtl;
        FlagIcon = flagIcon;
    }
}
