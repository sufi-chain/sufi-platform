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

    public Dictionary<Guid, string> TenantIdToPrimaryHost { get; set; } = new();

    /// <summary>Null on a cache entry written before public hosts were stored, which forces a rebuild.</summary>
    public List<TenantPublicHost>? PublicHosts { get; set; }
}
