using Microsoft.AspNetCore.Authentication;

namespace SufiChain.SufiPlatform.AspNetCore.Authentication.Server;

/// <summary>
/// Remember me issues a persistent cookie. Without it the ticket stays a session cookie.
/// </summary>
public static class LoginCookiePersistence
{
    public static AuthenticationProperties Create(bool rememberMe) =>
        new()
        {
            IsPersistent = rememberMe
        };
}
