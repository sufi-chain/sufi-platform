using Microsoft.Extensions.Caching.Distributed;

namespace SufiChain.SufiPlatform.Features;

/// <summary>
/// Feature values are cached without a tenant prefix (<see cref="FeatureValueCacheItem"/> is
/// <c>[IgnoreMultiTenancy]</c>). A missed invalidation must not keep the previous value
/// until process restart, so every write carries an absolute expiration.
/// </summary>
public static class FeatureValueCachePolicy
{
    public static readonly TimeSpan AbsoluteExpiration = TimeSpan.FromMinutes(10);

    public static DistributedCacheEntryOptions Create()
    {
        return new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = AbsoluteExpiration
        };
    }
}
