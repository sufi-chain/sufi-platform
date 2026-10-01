using System.Collections.Generic;
using Volo.Abp.Caching;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

[CacheName("SufiAI-ProviderModelList")]
public class ProviderModelListCacheItem
{
    public List<OpenAIModelDto> Models { get; set; } = new();

    public bool Unavailable { get; set; }

    /// <summary>
    /// OpenRouter metadata loaded from the same connection base URL as <see cref="Models"/>.
    /// </summary>
    public List<ModelCatalogEntry>? Catalog { get; set; }

    public bool CatalogLoaded { get; set; }
}
