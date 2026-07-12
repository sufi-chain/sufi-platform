using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.AspNetCore.Authentication;

namespace SufiChain.SufiPlatform.AspNetCore.Authentication.WebAssembly;

/// <summary>
/// WebAssembly implementation of ISufiAccessTokenProvider.
/// Wraps the built-in IAccessTokenProvider for use in Sufi framework.
/// </summary>
public class SufiAccessTokenProvider : ISufiAccessTokenProvider
{
    private readonly IAccessTokenProvider _accessTokenProvider;
    private readonly SufiAuthenticationOptions _options;

    public SufiAccessTokenProvider(
        IAccessTokenProvider accessTokenProvider,
        IOptions<SufiAuthenticationOptions> options)
    {
        _accessTokenProvider = accessTokenProvider;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async ValueTask<SufiAccessTokenResult> RequestAccessTokenAsync()
    {
        var result = await _accessTokenProvider.RequestAccessToken();

        if (result.TryGetToken(out var token))
        {
            return SufiAccessTokenResult.Success(new SufiAccessToken
            {
                Value = token.Value,
                Expires = token.Expires,
                GrantedScopes = token.GrantedScopes?.ToList() ?? new List<string>()
            });
        }

        return SufiAccessTokenResult.RequiresRedirect(_options.WebAssemblyLoginUrl);
    }

    /// <inheritdoc />
    public async ValueTask<SufiAccessTokenResult> RequestAccessTokenAsync(SufiAccessTokenRequestOptions options)
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
            return SufiAccessTokenResult.Success(new SufiAccessToken
            {
                Value = token.Value,
                Expires = token.Expires,
                GrantedScopes = token.GrantedScopes?.ToList() ?? new List<string>()
            });
        }

        return SufiAccessTokenResult.RequiresRedirect(_options.WebAssemblyLoginUrl);
    }
}
