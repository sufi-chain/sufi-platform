using System.Collections.Generic;

namespace SufiChain.SufiPlatform.SufiAI.Catalog;

public class ModelCatalogCacheItem
{
    public string ProviderName { get; set; } = string.Empty;

    public List<ModelCatalogEntry> Models { get; set; } = new();

    public List<string> UnknownIds { get; set; } = new();

    public bool Unavailable { get; set; }
}
