using Volo.Abp.Caching;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

/// <summary>
/// Distributed invalidation stamp for process-local embedding generators.
/// Generators themselves are not serializable and must not be stored in distributed cache.
/// </summary>
[CacheName("SufiAI-WorkspaceEmbedderStamp")]
public class WorkspaceEmbedderCacheStamp
{
    public string Stamp { get; set; } = string.Empty;
}
