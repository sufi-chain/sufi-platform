namespace SufiChain.SufiAbp.UI.Theming;

/// <summary>
/// Specifies the display name of a theme.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ThemeNameAttribute : Attribute
{
    /// <summary>
    /// The display name of the theme.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Creates a new ThemeNameAttribute.
    /// </summary>
    /// <param name="name">The display name of the theme.</param>
    public ThemeNameAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets the theme name from a theme type.
    /// Returns the ThemeNameAttribute.Name if present, otherwise returns the type name.
    /// </summary>
    /// <param name="themeType">The theme type to get the name from.</param>
    /// <returns>The theme name.</returns>
    public static string GetName(Type themeType)
    {
        return themeType
            .GetCustomAttributes(true)
            .OfType<ThemeNameAttribute>()
            .FirstOrDefault()?.Name ?? themeType.Name;
    }
}
