using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using SufiChain.SufiPlatform.UI.Abstractions.Account;
using Volo.Abp.Caching;

namespace SufiChain.SufiPlatform.Account;

/// <summary>
/// Distributed-cache phone confirmation sessions. The same token is valid for state, send, and confirm
/// until it expires or the phone is confirmed.
/// </summary>
public class PhoneConfirmationSessionStore : IPhoneConfirmationSessionStore
{
    public const int LifetimeMinutes = 30;

    private const string KeyPrefix = "Account:PhoneConfirmSession:";

    protected IDistributedCache<PhoneConfirmationSessionCacheItem> Cache { get; }

    public PhoneConfirmationSessionStore(IDistributedCache<PhoneConfirmationSessionCacheItem> cache)
    {
        Cache = cache;
    }

    public bool IsSupported => true;

    public virtual async Task<string> CreateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        await Cache.SetAsync(
            KeyPrefix + token,
            new PhoneConfirmationSessionCacheItem { UserId = userId },
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(LifetimeMinutes)
            },
            token: cancellationToken);

        return token;
    }

    public virtual async Task<Guid?> FindUserIdAsync(string? sessionToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionToken))
        {
            return null;
        }

        var item = await Cache.GetAsync(KeyPrefix + sessionToken.Trim(), token: cancellationToken);
        return item == null || item.UserId == Guid.Empty ? null : item.UserId;
    }

    public virtual Task RevokeAsync(string? sessionToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionToken))
        {
            return Task.CompletedTask;
        }

        return Cache.RemoveAsync(KeyPrefix + sessionToken.Trim(), token: cancellationToken);
    }
}
