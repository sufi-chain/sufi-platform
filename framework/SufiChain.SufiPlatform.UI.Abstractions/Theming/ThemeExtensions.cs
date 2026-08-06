using SufiChain.SufiPlatform.UI.Layout;

namespace SufiChain.SufiPlatform.UI.Theming;

/// <summary>
/// Extension methods for ITheme.
/// </summary>
public static class ThemeExtensions
{
    /// <summary>
    /// Gets the Application layout from the theme.
    /// </summary>
    public static Type GetApplicationLayout(this ITheme theme, bool fallbackToDefault = true)
    {
        return theme.GetLayout(StandardLayouts.Application, fallbackToDefault);
    }

    /// <summary>
    /// Gets the Account layout from the theme.
    /// </summary>
    public static Type GetAccountLayout(this ITheme theme, bool fallbackToDefault = true)
    {
        return theme.GetLayout(StandardLayouts.Account, fallbackToDefault);
    }

    /// <summary>
    /// Gets the Public layout from the theme.
    /// </summary>
    public static Type GetPublicLayout(this ITheme theme, bool fallbackToDefault = true)
    {
        return theme.GetLayout(StandardLayouts.Public, fallbackToDefault);
    }

    /// <summary>
    /// Gets the Empty layout from the theme.
    /// </summary>
    public static Type GetEmptyLayout(this ITheme theme, bool fallbackToDefault = true)
    {
        return theme.GetLayout(StandardLayouts.Empty, fallbackToDefault);
    }
}
