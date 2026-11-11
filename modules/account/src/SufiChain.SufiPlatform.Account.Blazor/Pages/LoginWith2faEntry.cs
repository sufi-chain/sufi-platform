using Microsoft.AspNetCore.Http;

namespace SufiChain.SufiPlatform.Account.Blazor.Pages;

/// <summary>
/// GET /account/login-2fa with no pending session has nothing to render.
/// Send the browser back to the login page and keep a local return path.
/// </summary>
public static class LoginWith2faEntry
{
    public static bool IsPage(PathString path)
    {
        var value = path.Value ?? string.Empty;
        return value.Equals("/account/login-2fa", StringComparison.OrdinalIgnoreCase)
            || value.EndsWith("/account/login-2fa", StringComparison.OrdinalIgnoreCase);
    }

    public static string LoginLocation(string? returnUrl)
    {
        if (!IsLocalReturn(returnUrl))
        {
            return "/account/login";
        }

        return "/account/login?returnUrl=" + Uri.EscapeDataString(returnUrl!);
    }

    public static bool IsLocalReturn(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl) || !returnUrl.StartsWith('/'))
        {
            return false;
        }

        return !returnUrl.StartsWith("//", StringComparison.Ordinal)
            && !returnUrl.StartsWith("/\\", StringComparison.Ordinal);
    }
}
