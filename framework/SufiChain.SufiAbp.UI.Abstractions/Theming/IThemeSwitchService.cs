namespace SufiChain.SufiAbp.UI.Theming;

/// <summary>
/// Service for managing theme switching (dark/light mode).
/// </summary>
public interface IThemeSwitchService
{
    /// <summary>
    /// Gets the current theme mode.
    /// </summary>
    ThemeMode CurrentMode { get; }

    /// <summary>
    /// Gets whether dark mode is currently active.
    /// </summary>
    bool IsDarkMode { get; }

    /// <summary>
    /// Event raised when the theme mode changes.
    /// </summary>
    event Action<ThemeMode>? ThemeChanged;

    /// <summary>
    /// Sets the theme mode.
    /// </summary>
    Task SetThemeModeAsync(ThemeMode mode);

    /// <summary>
    /// Toggles between light and dark mode.
    /// </summary>
    Task ToggleThemeAsync();

    /// <summary>
    /// Gets the stored theme preference from browser storage.
    /// </summary>
    Task<ThemeMode> GetStoredThemeAsync();
}

/// <summary>
/// Theme mode options.
/// </summary>
public enum ThemeMode
{
    /// <summary>
    /// Light theme.
    /// </summary>
    Light,

    /// <summary>
    /// Dark theme.
    /// </summary>
    Dark,

    /// <summary>
    /// Follow system preference.
    /// </summary>
    System
}
