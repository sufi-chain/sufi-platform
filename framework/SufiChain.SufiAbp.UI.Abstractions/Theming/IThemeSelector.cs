namespace SufiChain.SufiAbp.UI.Theming;

/// <summary>
/// Selects which theme should be used.
/// Can be customized to support user preferences, cookies, etc.
/// </summary>
public interface IThemeSelector
{
    /// <summary>
    /// Gets information about the currently selected theme.
    /// </summary>
    /// <returns>The theme information for the current context.</returns>
    ThemeInfo GetCurrentThemeInfo();
}
