using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Account.Otp;

public class OtpCodeStore : IOtpCodeStore, ITransientDependency
{
    private const string OtpKeyPrefix = "Account:Otp:";
    private const string RateLimitKeyPrefix = "Account:OtpRate:";
    private const string RegistrationTokenKeyPrefix = "Account:OtpRegToken:";

    protected IDistributedCache<OtpCacheItem> OtpCache { get; }

    protected IDistributedCache<OtpRateLimitCacheItem> RateLimitCache { get; }

    protected IDistributedCache<string> RegistrationTokenCache { get; }

    public OtpCodeStore(
        IDistributedCache<OtpCacheItem> otpCache,
        IDistributedCache<OtpRateLimitCacheItem> rateLimitCache,
        IDistributedCache<string> registrationTokenCache)
    {
        OtpCache = otpCache;
        RateLimitCache = rateLimitCache;
        RegistrationTokenCache = registrationTokenCache;
    }

    public virtual async Task StoreAsync(
        VerificationPurpose purpose,
        VerificationDeliveryChannel channel,
        string identifier,
        OtpCacheItem item,
        int expirationMinutes,
        CancellationToken cancellationToken = default)
    {
        await OtpCache.SetAsync(
            BuildOtpKey(purpose, channel, identifier),
            item,
            new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes)
            },
            token: cancellationToken);
    }

    public virtual Task<OtpCacheItem?> GetAsync(
        VerificationPurpose purpose,
        VerificationDeliveryChannel channel,
        string identifier,
        CancellationToken cancellationToken = default)
    {
        return OtpCache.GetAsync(BuildOtpKey(purpose, channel, identifier), token: cancellationToken);
    }

    public virtual Task RemoveAsync(
        VerificationPurpose purpose,
        VerificationDeliveryChannel channel,
        string identifier,
        CancellationToken cancellationToken = default)
    {
        return OtpCache.RemoveAsync(BuildOtpKey(purpose, channel, identifier), token: cancellationToken);
    }

    public virtual async Task<bool> TryIncrementRateLimitAsync(
        VerificationPurpose purpose,
        VerificationDeliveryChannel channel,
        string identifier,
        int maxPerHour,
        CancellationToken cancellationToken = default)
    {
        if (maxPerHour <= 0)
        {
            return true;
        }

        var key = BuildRateLimitKey(purpose, channel, identifier);
        var cacheItem = await RateLimitCache.GetAsync(key, token: cancellationToken);
        var count = cacheItem?.Count ?? 0;
        if (count >= maxPerHour)
        {
            return false;
        }

        await RateLimitCache.SetAsync(
            key,
            new OtpRateLimitCacheItem { Count = count + 1 },
            new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            },
            token: cancellationToken);

        return true;
    }

    public virtual async Task<string> CreateRegistrationTokenAsync(
        string email,
        int expirationMinutes,
        CancellationToken cancellationToken = default)
    {
        var token = Guid.NewGuid().ToString("N");
        await RegistrationTokenCache.SetAsync(
            RegistrationTokenKeyPrefix + token,
            NormalizeIdentifier(email),
            new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes)
            },
            token: cancellationToken);

        return token;
    }

    public virtual async Task<string?> ConsumeRegistrationTokenAsync(
        string registrationToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(registrationToken))
        {
            return null;
        }

        var key = RegistrationTokenKeyPrefix + registrationToken.Trim();
        var email = await RegistrationTokenCache.GetAsync(key, token: cancellationToken);
        if (email == null)
        {
            return null;
        }

        await RegistrationTokenCache.RemoveAsync(key, token: cancellationToken);
        return email;
    }

    protected static string BuildOtpKey(
        VerificationPurpose purpose,
        VerificationDeliveryChannel channel,
        string identifier)
    {
        return $"{OtpKeyPrefix}{purpose}:{channel}:{NormalizeIdentifier(identifier)}";
    }

    protected static string BuildRateLimitKey(
        VerificationPurpose purpose,
        VerificationDeliveryChannel channel,
        string identifier)
    {
        return $"{RateLimitKeyPrefix}{purpose}:{channel}:{NormalizeIdentifier(identifier)}";
    }

    protected static string NormalizeIdentifier(string identifier)
    {
        return identifier.Trim().ToLowerInvariant();
    }
}
