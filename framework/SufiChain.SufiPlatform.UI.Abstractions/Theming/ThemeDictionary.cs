namespace SufiChain.SufiPlatform.UI.Theming;

/// <summary>
/// A dictionary of registered themes, keyed by theme type.
/// </summary>
public class ThemeDictionary : Dictionary<Type, ThemeInfo>
{
    /// <summary>
    /// Registers a theme by its type.
    /// </summary>
    /// <typeparam name="TTheme">The theme type to register.</typeparam>
    /// <returns>The ThemeInfo for the registered theme.</returns>
    public ThemeInfo Add<TTheme>() where TTheme : ITheme
    {
        return Add(typeof(TTheme));
    }

    /// <summary>
    /// Registers a theme by its type.
    /// </summary>
    /// <param name="themeType">The theme type to register.</param>
    /// <returns>The ThemeInfo for the registered theme.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the theme is already registered.</exception>
    public ThemeInfo Add(Type themeType)
    {
        if (ContainsKey(themeType))
        {
            throw new InvalidOperationException(
                $"Theme is already registered: {themeType.AssemblyQualifiedName}");
        }

        return this[themeType] = new ThemeInfo(themeType);
    }
}
