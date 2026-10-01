using System.Collections.Generic;
using SufiChain.SufiPlatform.SufiAI;
using Volo.Abp.Caching;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

/// <summary>
/// OpenRouter capability catalog for one connection base URL.
/// Shared by every workspace that uses that gateway.
/// </summary>
[CacheName("SufiAI-OpenRouterConnectionCatalog")]
public class OpenRouterConnectionCatalogCacheItem
{
    public List<ModelCatalogEntry> Models { get; set; } = new();

    public bool Unavailable { get; set; }
}
