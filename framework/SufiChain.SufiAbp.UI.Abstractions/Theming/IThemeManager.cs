namespace SufiChain.SufiAbp.UI.Theming;

/// <summary>
/// Manages the current theme for the application.
/// </summary>
public interface IThemeManager
{
    /// <summary>
    /// Gets the currently active theme.
    /// </summary>
    ITheme CurrentTheme { get; }
}
