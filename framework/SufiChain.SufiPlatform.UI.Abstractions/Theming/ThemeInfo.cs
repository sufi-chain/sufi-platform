namespace SufiChain.SufiPlatform.UI.Theming;

/// <summary>
/// Contains metadata about a registered theme.
/// </summary>
public class ThemeInfo
{
    /// <summary>
    /// The Type of the theme class.
    /// </summary>
    public Type ThemeType { get; }

    /// <summary>
    /// The display name of the theme.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Creates a new ThemeInfo instance.
    /// </summary>
    /// <param name="themeType">The Type of the theme class. Must implement ITheme.</param>
    /// <exception cref="ArgumentNullException">Thrown when themeType is null.</exception>
    /// <exception cref="ArgumentException">Thrown when themeType does not implement ITheme.</exception>
    public ThemeInfo(Type themeType)
    {
        if (themeType == null)
        {
            throw new ArgumentNullException(nameof(themeType));
        }

        if (!typeof(ITheme).IsAssignableFrom(themeType))
        {
            throw new ArgumentException(
                $"Given {nameof(themeType)} ({themeType.AssemblyQualifiedName}) must implement {typeof(ITheme).FullName}",
                nameof(themeType));
        }

        ThemeType = themeType;
        Name = ThemeNameAttribute.GetName(themeType);
    }
}
