namespace SufiChain.SufiAbp.Authorization.Permissions;

internal static class LocalizableStringConverter
{
    public static Volo.Abp.Localization.ILocalizableString? ToVolo(object? displayName)
    {
        if (displayName == null)
        {
            return null;
        }

        if (displayName is Volo.Abp.Localization.ILocalizableString localizableString)
        {
            return localizableString;
        }

        var toVoloMethod = displayName.GetType().GetMethod("ToVolo", Type.EmptyTypes);
        if (toVoloMethod?.Invoke(displayName, null) is Volo.Abp.Localization.ILocalizableString converted)
        {
            return converted;
        }

        throw new ArgumentException($"Unsupported localizable string type: {displayName.GetType().FullName}", nameof(displayName));
    }
}
