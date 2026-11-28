using Microsoft.AspNetCore.Http;

namespace SufiChain.SufiPlatform.AspNetCore.Authentication.Server;

/// <summary>
/// Writes the culture cookie from the signed-in user's saved language.
/// Hosts that have a user-language store register an implementation.
/// </summary>
public interface ILoginCultureCookieWriter
{
    Task AppendForUserAsync(HttpContext httpContext, Guid userId);
}
