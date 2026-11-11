using Microsoft.Extensions.Localization;

namespace SufiChain.SufiPlatform.Account.Blazor.Pages;

/// <summary>
/// Turns an <c>error</c> query value into a sentence. Known aliases map to resource keys.
/// A key the identity resource has is used as-is; otherwise the account resource is tried.
/// </summary>
public static class LoginQueryErrors
{
    public static string Localize(string key, IStringLocalizer identity, IStringLocalizer account)
    {
        key = key switch
        {
            "EmailConfirmationRequired" => "Sufi.Identity:EmailConfirmationRequired",
            "InvalidOrExpiredToken" or "UserNotFound" => "LoginTokenExpired",
            "LinkLoginFailed" => "LoginLinkFailed",
            _ => key
        };

        var localized = identity[key];
        if (!localized.ResourceNotFound)
        {
            return localized.Value;
        }

        var accountLocalized = account[key];
        return accountLocalized.ResourceNotFound ? identity["LoginUnexpectedError"].Value : accountLocalized.Value;
    }
}
