using Microsoft.Extensions.Caching.Memory;
using SufiChain.SufiAbp.UI.Abstractions.Account;

namespace SufiChain.SufiAbp.AspNetCore.Authentication.Server;

/// <summary>
/// Default IMemoryCache-based one-time token store for login completion.
/// Used so the Blazor Interactive Server circuit can redirect to a GET endpoint
/// that sets the auth cookie in a normal HTTP request.
/// Registered by <see cref="SufiAbpAuthenticationServerModule"/>.
/// </summary>
public class LoginCompletionTokenStore : ILoginCompletionTokenStore
{
    private const string CacheKeyPrefix = "LoginCompletion:";
    private static readonly TimeSpan TokenExpiry = TimeSpan.FromMinutes(1);
    private readonly IMemoryCache _cache;

    public bool IsSupported => true;

    public LoginCompletionTokenStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<string> CreateAsync(Guid userId, string? returnUrl, bool rememberMe = false, CancellationToken cancellationToken = default)
    {
        var token = Guid.NewGuid().ToString("N");
        var key = CacheKeyPrefix + token;
        var value = (userId, returnUrl, rememberMe);
        _cache.Set(key, value, TokenExpiry);
        return Task.FromResult(token);
    }

    public Task<(Guid userId, string? returnUrl, bool rememberMe)?> ConsumeAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Task.FromResult<(Guid, string?, bool)?>(null);

        var key = CacheKeyPrefix + token.Trim();
        if (_cache.TryGetValue(key, out (Guid userId, string? returnUrl, bool rememberMe) value))
        {
            _cache.Remove(key);
            return Task.FromResult<(Guid, string?, bool)?>(value);
        }

        return Task.FromResult<(Guid, string?, bool)?>(null);
    }
}
