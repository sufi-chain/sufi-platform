namespace SufiChain.SufiAbp.UI.Theming;

/// <summary>
/// Options for configuring theming.
/// </summary>
public class ThemingOptions
{
    /// <summary>
    /// Dictionary of registered themes.
    /// </summary>
    public ThemeDictionary Themes { get; }

    /// <summary>
    /// The name of the default theme. If null, the first registered theme is used.
    /// </summary>
    public string? DefaultThemeName { get; set; }

    /// <summary>
    /// Creates a new ThemingOptions instance.
    /// </summary>
    public ThemingOptions()
    {
        Themes = new ThemeDictionary();
    }
}
