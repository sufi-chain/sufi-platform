namespace SufiChain.SufiPlatform.UI.Theming;

/// <summary>
/// Interface for theme implementations.
/// Themes provide layouts for different layout contexts (Application, Account, etc.).
/// Supports both MVC (cshtml paths) and Blazor (component types).
/// </summary>
public interface ITheme
{
    /// <summary>
    /// Gets the layout component type for the specified layout name (Blazor).
    /// </summary>
    /// <param name="name">The layout name (e.g., StandardLayouts.Application).</param>
    /// <param name="fallbackToDefault">If true, returns the default layout when the specified layout is not found.</param>
    /// <returns>The Type of the layout component, or null if not supported.</returns>
    Type? GetLayout(string name, bool fallbackToDefault = true);

    /// <summary>
    /// Gets the layout path for the specified layout name (MVC/Razor Pages).
    /// </summary>
    /// <param name="name">The layout name (e.g., StandardLayouts.Application).</param>
    /// <param name="fallbackToDefault">If true, returns the default layout when the specified layout is not found.</param>
    /// <returns>The path to the layout cshtml file, or null if not supported.</returns>
    string? GetLayoutPath(string name, bool fallbackToDefault = true);
}
