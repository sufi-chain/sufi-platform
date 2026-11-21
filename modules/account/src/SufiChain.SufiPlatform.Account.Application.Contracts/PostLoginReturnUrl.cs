namespace SufiChain.SufiPlatform.Account;

/// <summary>
/// Keeps post-login redirects off leftover manage-profile tabs (HST-05).
/// Forced password change still uses the password tab via
/// <c>ShouldChangePasswordOnNextLogin</c>, not a stale return URL.
/// </summary>
public static class PostLoginReturnUrl
{
    public const string DashboardPath = "/panel/dashboard";
    public const string ForcedPasswordChangePath = "/panel/portal/profile?tab=password";

    private static readonly string[] HostCultures = ["en", "fa", "ar", "es"];

    public static bool IsStalePasswordTab(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return false;
        }

        var decoded = Uri.UnescapeDataString(returnUrl.Trim());
        var pathAndQuery = decoded.Split('#', 2)[0];
        var parts = pathAndQuery.Split('?', 2);
        var path = StripCulturePrefix(parts[0].TrimEnd('/'));

        if (!path.Equals("/panel/portal/profile", StringComparison.OrdinalIgnoreCase)
            && !path.Equals("/panel/account/manage", StringComparison.OrdinalIgnoreCase)
            && !path.Equals("/account/manage", StringComparison.OrdinalIgnoreCase)
            && !path.Equals("/account/change-password", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (parts.Length == 1)
        {
            return path.Contains("change-password", StringComparison.OrdinalIgnoreCase);
        }

        var query = parts[1];
        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = pair.Split('=', 2);
            if (kv.Length == 2
                && kv[0].Equals("tab", StringComparison.OrdinalIgnoreCase)
                && Uri.UnescapeDataString(kv[1]).Equals("password", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return path.Contains("change-password", StringComparison.OrdinalIgnoreCase);
    }

    public static string Resolve(string? returnUrl, bool forcePasswordChange = false)
    {
        if (forcePasswordChange)
        {
            return ForcedPasswordChangePath;
        }

        if (string.IsNullOrWhiteSpace(returnUrl)
            || returnUrl is "/" or "~/" or "%2F"
            || IsStalePasswordTab(returnUrl))
        {
            return DashboardPath;
        }

        return returnUrl;
    }

    private static string StripCulturePrefix(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return path;
        }

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2)
        {
            return path;
        }

        var culture = segments[0];
        if (!HostCultures.Any(c => c.Equals(culture, StringComparison.OrdinalIgnoreCase)))
        {
            return path;
        }

        if (!segments[1].Equals("panel", StringComparison.OrdinalIgnoreCase)
            && !segments[1].Equals("account", StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }

        return "/" + string.Join('/', segments.Skip(1));
    }
}
