namespace SufiChain.SufiPlatform.Account.Blazor;

/// <summary>
/// Builds the create-link browser path: sign out, sign in as the other account, then open the completion page.
/// The completion page verifies the original user's link token and calls <c>LinkAsync</c>.
/// </summary>
internal static class LinkAccountHandshake
{
    public const string CompletionPath = "/account/link-user";

    public static string BuildCompletionPath(Guid linkUserId, Guid? linkTenantId, string token)
    {
        var path =
            $"{CompletionPath}?linkUserId={linkUserId}&linkToken={Uri.EscapeDataString(token)}";

        if (linkTenantId.HasValue)
        {
            path += $"&linkTenantId={linkTenantId.Value}";
        }

        return path;
    }

    public static string BuildSecondaryLoginPath(string completionPath)
    {
        return BuildSignInPath("/account/login", createLink: true, completionPath);
    }

    public static string BuildLogoutPath(string loginPath)
    {
        return "/account/logout?returnUrl=" + Uri.EscapeDataString(loginPath);
    }

    public static string BuildSignInPath(string path, bool createLink, string? returnUrl)
    {
        var query = new List<string>();
        if (createLink)
        {
            query.Add("createLink=true");
        }

        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            query.Add("returnUrl=" + Uri.EscapeDataString(returnUrl));
        }

        if (query.Count == 0)
        {
            return path;
        }

        return path + "?" + string.Join("&", query);
    }
}
