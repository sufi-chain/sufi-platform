namespace SufiChain.SufiPlatform.Menus.Menus;

public static class MenuItemUrlRules
{
    public static bool IsAcceptable(MenuItemKind kind, string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return kind is not MenuItemKind.InternalRoute and not MenuItemKind.ExternalUrl;
        }

        var value = url.Trim();
        if (value.Contains(' ') || value.Contains('\\') || value.Contains('<') || value.Contains('>'))
        {
            return false;
        }

        if (value.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return kind switch
        {
            MenuItemKind.InternalRoute => value.StartsWith('/') && !value.StartsWith("//"),
            MenuItemKind.ExternalUrl => Uri.TryCreate(value, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps),
            _ => true
        };
    }
}
