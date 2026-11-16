using System.Text.Json.Serialization;
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
    private bool? _seededIsDark;
    private int _writeGeneration;

    public ThemeMode CurrentMode => _currentMode;
    public bool IsPreferenceResolved => _initialized;

    public bool IsDarkMode =>
        _initialized
            ? ResolveIsDark(_currentMode, _systemPrefersDark)
            : _seededIsDark ?? ResolveIsDark(_currentMode, _systemPrefersDark);

    public event Action<ThemeMode>? ThemeChanged;

    public ThemeSwitchService(ILocalStorageService localStorage, IJSRuntime jsRuntime)
    {
        _localStorage = localStorage;
        _jsRuntime = jsRuntime;
    }

    public void SeedResolved(bool isDark)
    {
        _seededIsDark = isDark;
    }

    public async Task SetThemeModeAsync(ThemeMode mode)
    {
        var write = ++_writeGeneration;
        _currentMode = mode;
        _initialized = true;
        _seededIsDark = null;

        await _localStorage.SetItemAsync(StorageKey, mode.ToString());
        if (write != _writeGeneration)
        {
            return;
        }

        await ApplyThemeToDomAsync(force: true, mode: mode);

        ThemeChanged?.Invoke(mode);
    }

    public async Task ToggleThemeAsync()
    {
        var newMode = _currentMode switch
        {
            ThemeMode.Light => ThemeMode.Dark,
            ThemeMode.Dark => ThemeMode.Light,
            ThemeMode.System => ThemeMode.Dark,
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

        var seededIsDark = _seededIsDark;
        var seenWrite = _writeGeneration;

        try
        {
            var state = await _jsRuntime.InvokeAsync<SufiThemeJsState>("sufiTheme.getState");
            if (seenWrite != _writeGeneration)
            {
                return _currentMode;
            }

            _systemPrefersDark = state.SystemDark;
            _currentMode = ResolveStoredMode(state.Stored, seededIsDark);
            _initialized = true;

            var resolvedIsDark = ResolveIsDark(_currentMode, _systemPrefersDark);
            var skipDomApply = seededIsDark is bool seed && seed == resolvedIsDark;
            if (!skipDomApply)
            {
                await ApplyThemeToDomAsync(force: false, isLightOverride: !resolvedIsDark);
            }
        }
        catch
        {
            return _currentMode;
        }

        return _currentMode;
    }

    private static bool ResolveIsDark(ThemeMode mode, bool systemPrefersDark) =>
        mode == ThemeMode.Dark || (mode == ThemeMode.System && systemPrefersDark);

    private static ThemeMode ResolveStoredMode(string? stored, bool? seededIsDark)
    {
        if (!string.IsNullOrWhiteSpace(stored)
            && Enum.TryParse<ThemeMode>(stored.Trim().Trim('"'), ignoreCase: true, out var mode))
        {
            return mode;
        }

        // A request cookie already chose the appearance. Do not replace it with
        // the operating-system preference when storage is empty or unreadable.
        return seededIsDark switch
        {
            true => ThemeMode.Dark,
            false => ThemeMode.Light,
            _ => ThemeMode.System
        };
    }

    private async Task ApplyThemeToDomAsync(bool force, bool? isLightOverride = null, ThemeMode? mode = null)
    {
        try
        {
            var isLight = isLightOverride ?? !IsDarkMode;
            try
            {
                await _jsRuntime.InvokeVoidAsync("sufiTheme.apply", isLight, force, mode?.ToString());
            }
            catch (JSException)
            {
                if (!force)
                {
                    return;
                }

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

/// <summary>
/// Theme state returned from <c>sufiTheme.getState()</c>.
/// </summary>
public sealed class SufiThemeJsState
{
    [JsonPropertyName("stored")]
    public string? Stored { get; set; }

    [JsonPropertyName("systemDark")]
    public bool SystemDark { get; set; }

    [JsonPropertyName("isLight")]
    public bool IsLight { get; set; }
}
