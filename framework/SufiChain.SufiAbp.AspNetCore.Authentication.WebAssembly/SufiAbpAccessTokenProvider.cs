using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.AspNetCore.Authentication;

namespace SufiChain.SufiAbp.AspNetCore.Authentication.WebAssembly;

/// <summary>
/// WebAssembly implementation of ISufiAbpAccessTokenProvider.
/// Wraps the built-in IAccessTokenProvider for use in SufiAbp framework.
/// </summary>
public class SufiAbpAccessTokenProvider : ISufiAbpAccessTokenProvider
{
    private readonly IAccessTokenProvider _accessTokenProvider;
    private readonly SufiAbpAuthenticationOptions _options;

    public SufiAbpAccessTokenProvider(
        IAccessTokenProvider accessTokenProvider,
        IOptions<SufiAbpAuthenticationOptions> options)
    {
        _accessTokenProvider = accessTokenProvider;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async ValueTask<SufiAbpAccessTokenResult> RequestAccessTokenAsync()
    {
        var result = await _accessTokenProvider.RequestAccessToken();

        if (result.TryGetToken(out var token))
        {
            return SufiAbpAccessTokenResult.Success(new SufiAbpAccessToken
            {
                Value = token.Value,
                Expires = token.Expires,
                GrantedScopes = token.GrantedScopes?.ToList() ?? new List<string>()
            });
        }

        return SufiAbpAccessTokenResult.RequiresRedirect(_options.WebAssemblyLoginUrl);
    }

    /// <inheritdoc />
    public async ValueTask<SufiAbpAccessTokenResult> RequestAccessTokenAsync(SufiAbpAccessTokenRequestOptions options)
    {
        var requestOptions = new AccessTokenRequestOptions();

        if (options.Scopes != null)
        {
            requestOptions.Scopes = options.Scopes;
        }

        if (!string.IsNullOrEmpty(options.ReturnUrl))
        {
            requestOptions.ReturnUrl = options.ReturnUrl;
        }

        var result = await _accessTokenProvider.RequestAccessToken(requestOptions);

        if (result.TryGetToken(out var token))
        {
            return SufiAbpAccessTokenResult.Success(new SufiAbpAccessToken
            {
                Value = token.Value,
                Expires = token.Expires,
                GrantedScopes = token.GrantedScopes?.ToList() ?? new List<string>()
            });
        }

        return SufiAbpAccessTokenResult.RequiresRedirect(_options.WebAssemblyLoginUrl);
    }
}
