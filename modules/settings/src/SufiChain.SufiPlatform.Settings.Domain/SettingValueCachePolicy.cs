using Microsoft.Extensions.Caching.Distributed;

namespace SufiChain.SufiPlatform.Settings;

/// <summary>
/// Setting values are cached without a tenant prefix (<see cref="SettingCacheItem"/> is
/// <c>[IgnoreMultiTenancy]</c>). A missed invalidation must not keep the previous value
/// until process restart, so every write carries an absolute expiration.
/// </summary>
public static class SettingValueCachePolicy
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
