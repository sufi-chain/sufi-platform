using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.UI.Theming;

namespace SufiChain.SufiPlatform.UI.Services.Theming;

/// <summary>
/// Default implementation of IThemeManager.
/// </summary>
public class DefaultThemeManager : IThemeManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IThemeSelector _themeSelector;
    private ITheme? _currentTheme;

    /// <inheritdoc/>
    public ITheme CurrentTheme => GetCurrentTheme();

    public DefaultThemeManager(
        IServiceProvider serviceProvider,
        IThemeSelector themeSelector)
    {
        _serviceProvider = serviceProvider;
        _themeSelector = themeSelector;
    }

    protected virtual ITheme GetCurrentTheme()
    {
        if (_currentTheme != null)
        {
            return _currentTheme;
        }

        var themeInfo = _themeSelector.GetCurrentThemeInfo();
        _currentTheme = (ITheme)_serviceProvider.GetRequiredService(themeInfo.ThemeType);
        return _currentTheme;
    }
}
