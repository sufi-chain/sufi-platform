using Microsoft.JSInterop;
using SufiChain.SufiPlatform.UI.Browser;
using SufiChain.SufiPlatform.UI.Theming;

namespace SufiChain.SufiPlatform.UI.Blazor.Theming;

/// <summary>
/// Implementation of IThemeSwitchService that persists preference to local storage.
/// </summary>
public class ThemeSwitchService : IThemeSwitchService
{
    public const string StorageKey = "sufi-theme-mode";
    public const string ResolvedCookieName = "sufi-theme-resolved";

    private readonly ILocalStorageService _localStorage;
    private readonly IJSRuntime _jsRuntime;
    private ThemeMode _currentMode = ThemeMode.System;
    private bool _systemPrefersDark;
    private bool _initialized;

    public ThemeMode CurrentMode => _currentMode;
    public bool IsPreferenceResolved => _initialized;
    public bool IsDarkMode => _currentMode == ThemeMode.Dark ||
                              (_currentMode == ThemeMode.System && _systemPrefersDark);

    public event Action<ThemeMode>? ThemeChanged;

    public ThemeSwitchService(ILocalStorageService localStorage, IJSRuntime jsRuntime)
    {
        _localStorage = localStorage;
        _jsRuntime = jsRuntime;
    }

    public async Task SetThemeModeAsync(ThemeMode mode)
    {
        _currentMode = mode;
        
        // Persist to local storage
        await _localStorage.SetItemAsync(StorageKey, mode.ToString());
        
        // Apply theme to DOM
        await ApplyThemeToDomAsync();
        
        // Notify listeners
        ThemeChanged?.Invoke(mode);
    }

    public async Task ToggleThemeAsync()
    {
        var newMode = _currentMode switch
        {
            ThemeMode.Light => ThemeMode.Dark,
            ThemeMode.Dark => ThemeMode.Light,
            ThemeMode.System => ThemeMode.Dark, // Toggle to explicit dark
            _ => ThemeMode.Light
        };

        await SetThemeModeAsync(newMode);
    }

    public async Task<ThemeMode> GetStoredThemeAsync()
    {
        if (_initialized)
        {
            return _currentMode;
        }

        try
        {
            // Detect system preference via JS interop
            _systemPrefersDark = await DetectSystemDarkModeAsync();

            var stored = await _localStorage.GetItemAsync(StorageKey);
            if (!string.IsNullOrEmpty(stored) && Enum.TryParse<ThemeMode>(stored, out var mode))
            {
                _currentMode = mode;
            }
            else
            {
                // No explicit choice yet: follow the operating system.
                _currentMode = ThemeMode.System;
            }

            _initialized = true;
        }
        catch
        {
            // Browser storage is unavailable during prerender. Leave the mode unresolved
            // so a later call can read it, and do not replace a cookie-seeded theme.
            return _currentMode;
        }

        await ApplyThemeToDomAsync();
        return _currentMode;
    }

    /// <summary>
    /// Detects system dark mode preference via JS interop using matchMedia.
    /// </summary>
    private async Task<bool> DetectSystemDarkModeAsync()
    {
        return await _jsRuntime.InvokeAsync<bool>(
            "eval",
            "window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches");
    }

    private async Task ApplyThemeToDomAsync()
    {
        try
        {
            var isLight = !IsDarkMode;
            try
            {
                await _jsRuntime.InvokeVoidAsync("sufiTheme.apply", isLight);
            }
            catch (JSException)
            {
                await _jsRuntime.InvokeVoidAsync(
                    "eval",
                    isLight
                        ? "document.documentElement.classList.add('sufi-theme-light','sufi-startup-theme-light'); document.documentElement.classList.remove('sufi-theme-dark','sufi-startup-theme-dark','sb-theme-dark','sb-theme-light'); document.cookie='sufi-theme-resolved=Light;path=/;max-age=31536000;SameSite=Lax';"
                        : "document.documentElement.classList.add('sufi-theme-dark','sufi-startup-theme-dark'); document.documentElement.classList.remove('sufi-theme-light','sufi-startup-theme-light','sb-theme-dark','sb-theme-light'); document.cookie='sufi-theme-resolved=Dark;path=/;max-age=31536000;SameSite=Lax';");
            }
        }
        catch
        {
            // JS interop may not be available yet
        }
    }
}
