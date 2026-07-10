namespace SufiChain.SufiAbp.MenuManagement.Menus;

/// <summary>
/// Maps seeded menu keys and menu context types to owning localization resources.
/// </summary>
public static class MenuLocalizationRegistry
{
    private static readonly Dictionary<string, string> MenuKeyResourceNames =
        new(StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, string> ContextTypeResourceNames =
        new(StringComparer.OrdinalIgnoreCase);

    public static void RegisterMenuKey(string menuKey, string localizationResourceName)
    {
        if (string.IsNullOrWhiteSpace(menuKey))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(menuKey));
        }

        if (string.IsNullOrWhiteSpace(localizationResourceName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(localizationResourceName));
        }

        MenuKeyResourceNames[menuKey] = localizationResourceName;
    }

    public static void RegisterContextType(string contextType, string localizationResourceName)
    {
        if (string.IsNullOrWhiteSpace(contextType))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(contextType));
        }

        if (string.IsNullOrWhiteSpace(localizationResourceName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(localizationResourceName));
        }

        ContextTypeResourceNames[contextType] = localizationResourceName;
    }

    public static string? GetResourceName(string? menuKey, string? contextType = null)
    {
        if (!string.IsNullOrWhiteSpace(menuKey) &&
            MenuKeyResourceNames.TryGetValue(menuKey, out var menuResourceName))
        {
            return menuResourceName;
        }

        if (!string.IsNullOrWhiteSpace(contextType) &&
            ContextTypeResourceNames.TryGetValue(contextType, out var contextResourceName))
        {
            return contextResourceName;
        }

        return null;
    }
}
