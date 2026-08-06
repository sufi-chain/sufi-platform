using Microsoft.JSInterop;
using SufiChain.SufiPlatform.UI.Browser;
using SufiChain.SufiPlatform.UI.Theming;

namespace SufiChain.SufiPlatform.UI.Blazor.Theming;

/// <summary>
/// Implementation of IThemeSwitchService that persists preference to local storage.
/// </summary>
public class ThemeSwitchService : IThemeSwitchService
{
    private const string StorageKey = "sabp-theme-mode";
    
    private readonly ILocalStorageService _localStorage;
    private readonly IJSRuntime _jsRuntime;
    private ThemeMode _currentMode = ThemeMode.Light;
    private bool _systemPrefersDark;
    private bool _initialized;

    public ThemeMode CurrentMode => _currentMode;
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
                // Default to system preference
                _currentMode = ThemeMode.System;
            }
        }
        catch
        {
            _currentMode = ThemeMode.Light;
        }

        _initialized = true;
        await ApplyThemeToDomAsync();
        return _currentMode;
    }

    /// <summary>
    /// Detects system dark mode preference via JS interop using matchMedia.
    /// </summary>
    private async Task<bool> DetectSystemDarkModeAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("eval", 
                "window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches");
        }
        catch
        {
            // JS interop may not be available during prerendering
            return false;
        }
    }

    private async Task ApplyThemeToDomAsync()
    {
        try
        {
            var isDark = IsDarkMode;
            
            // Apply dark class to document element
            if (isDark)
            {
                await _jsRuntime.InvokeVoidAsync("eval", 
                    "document.documentElement.classList.add('sb-theme-dark'); document.documentElement.classList.remove('sb-theme-light');");
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("eval", 
                    "document.documentElement.classList.add('sb-theme-light'); document.documentElement.classList.remove('sb-theme-dark');");
            }
        }
        catch
        {
            // JS interop may not be available yet
        }
    }
}
