using System.Collections.Generic;
using Volo.Abp.Caching;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

[CacheName("SufiAI-ProviderModelList")]
public class ProviderModelListCacheItem
{
    public List<OpenAIModelDto> Models { get; set; } = new();
}
