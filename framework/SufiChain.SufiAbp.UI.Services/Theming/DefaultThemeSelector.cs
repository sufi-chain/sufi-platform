using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.UI.Theming;

namespace SufiChain.SufiAbp.UI.Services.Theming;

/// <summary>
/// Default implementation of IThemeSelector that selects themes based on configuration.
/// </summary>
public class DefaultThemeSelector : IThemeSelector
{
    protected ThemingOptions Options { get; }

    public DefaultThemeSelector(IOptions<ThemingOptions> options)
    {
        Options = options.Value;
    }

    /// <inheritdoc/>
    public virtual ThemeInfo GetCurrentThemeInfo()
    {
        if (Options.Themes.Count == 0)
        {
            throw new InvalidOperationException(
                $"No themes registered! Use {nameof(ThemingOptions)} to register themes.");
        }

        if (Options.DefaultThemeName == null)
        {
            return Options.Themes.Values.First();
        }

        var themeInfo = Options.Themes.Values.FirstOrDefault(t => t.Name == Options.DefaultThemeName);
        if (themeInfo == null)
        {
            throw new InvalidOperationException(
                $"Default theme '{Options.DefaultThemeName}' is configured but not found in registered themes.");
        }

        return themeInfo;
    }
}
