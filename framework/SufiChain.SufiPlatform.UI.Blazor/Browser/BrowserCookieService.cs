using Microsoft.JSInterop;
using SufiChain.SufiPlatform.UI.Browser;

namespace SufiChain.SufiPlatform.UI.Blazor.Browser;

/// <summary>
/// Implementation of ICookieService using JavaScript interop.
/// </summary>
public class BrowserCookieService : ICookieService
{
    private readonly IJSRuntime _jsRuntime;

    public BrowserCookieService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async ValueTask SetAsync(string key, string value, UI.Browser.CookieOptions? options = null)
    {
        var cookieString = $"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}";

        if (options != null)
        {
            if (options.Expires.HasValue)
            {
                cookieString += $"; expires={options.Expires.Value.UtcDateTime:R}";
            }

            if (options.MaxAge.HasValue)
            {
                cookieString += $"; max-age={options.MaxAge.Value}";
            }

            if (!string.IsNullOrEmpty(options.Path))
            {
                cookieString += $"; path={options.Path}";
            }

            if (!string.IsNullOrEmpty(options.Domain))
            {
                cookieString += $"; domain={options.Domain}";
            }

            if (options.Secure)
            {
                cookieString += "; secure";
            }

            cookieString += options.SameSite switch
            {
                CookieSameSiteMode.Strict => "; samesite=strict",
                CookieSameSiteMode.Lax => "; samesite=lax",
                CookieSameSiteMode.None => "; samesite=none",
                _ => ""
            };
        }

        await _jsRuntime.InvokeVoidAsync("eval", $"document.cookie = '{cookieString}'");
    }

    public async ValueTask<string?> GetAsync(string key)
    {
        var cookies = await _jsRuntime.InvokeAsync<string>("eval", "document.cookie");
        
        if (string.IsNullOrEmpty(cookies))
        {
            return null;
        }

        var encodedKey = Uri.EscapeDataString(key);
        foreach (var cookie in cookies.Split(';'))
        {
            var trimmedCookie = cookie.Trim();
            if (trimmedCookie.StartsWith($"{encodedKey}=", StringComparison.Ordinal))
            {
                var value = trimmedCookie.Substring(encodedKey.Length + 1);
                return Uri.UnescapeDataString(value);
            }
        }

        return null;
    }

    public async ValueTask DeleteAsync(string key, string? path = null)
    {
        var cookieString = $"{Uri.EscapeDataString(key)}=; expires=Thu, 01 Jan 1970 00:00:00 UTC";
        
        if (!string.IsNullOrEmpty(path))
        {
            cookieString += $"; path={path}";
        }
        else
        {
            cookieString += "; path=/";
        }

        await _jsRuntime.InvokeVoidAsync("eval", $"document.cookie = '{cookieString}'");
    }
}
