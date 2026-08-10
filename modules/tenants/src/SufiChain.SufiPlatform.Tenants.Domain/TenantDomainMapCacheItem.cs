using System;
using System.Collections.Generic;
using Volo.Abp.Caching;

namespace SufiChain.SufiPlatform.Tenants;

[Serializable]
[CacheName("SufiTenantDomainMap")]
public class TenantDomainMapCacheItem
{
    public const string CacheKey = "All";

    public Dictionary<string, string> HostToTenantName { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);
}
