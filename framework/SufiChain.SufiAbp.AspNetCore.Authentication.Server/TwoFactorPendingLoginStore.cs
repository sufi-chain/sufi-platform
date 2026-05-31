using Microsoft.Extensions.Caching.Memory;
using SufiChain.SufiAbp.UI.Abstractions.Account;

namespace SufiChain.SufiAbp.AspNetCore.Authentication.Server;

/// <summary>
/// In-memory store for interactive-server two-factor login completion.
/// </summary>
public class TwoFactorPendingLoginStore : ITwoFactorPendingLoginStore
{
    private const string CacheKeyPrefix = "TwoFactorPending:";
    private static readonly TimeSpan TokenExpiry = TimeSpan.FromMinutes(5);
    private readonly IMemoryCache _cache;

    public bool IsSupported => true;

    public TwoFactorPendingLoginStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<string> CreateAsync(
        Guid userId,
        string? returnUrl,
        bool rememberMe = false,
        CancellationToken cancellationToken = default)
    {
        var token = Guid.NewGuid().ToString("N");
        var key = CacheKeyPrefix + token;
        _cache.Set(key, (userId, returnUrl, rememberMe), TokenExpiry);
        return Task.FromResult(token);
    }

    public Task<(Guid userId, string? returnUrl, bool rememberMe)?> GetAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult<(Guid, string?, bool)?>(null);
        }

        var key = CacheKeyPrefix + token.Trim();
        if (_cache.TryGetValue(key, out (Guid userId, string? returnUrl, bool rememberMe) value))
        {
            return Task.FromResult<(Guid, string?, bool)?>(value);
        }

        return Task.FromResult<(Guid, string?, bool)?>(null);
    }

    public Task<(Guid userId, string? returnUrl, bool rememberMe)?> ConsumeAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult<(Guid, string?, bool)?>(null);
        }

        var key = CacheKeyPrefix + token.Trim();
        if (_cache.TryGetValue(key, out (Guid userId, string? returnUrl, bool rememberMe) value))
        {
            _cache.Remove(key);
            return Task.FromResult<(Guid, string?, bool)?>(value);
        }

        return Task.FromResult<(Guid, string?, bool)?>(null);
    }
}
