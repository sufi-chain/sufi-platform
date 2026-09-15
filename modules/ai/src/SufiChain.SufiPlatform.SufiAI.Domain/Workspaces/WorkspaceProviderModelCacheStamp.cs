using Volo.Abp.Caching;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

/// <summary>
/// Distributed invalidation stamp for cached OpenAI-compatible model lists.
/// Credential material is never stored; callers key by a salted fingerprint prefix.
/// </summary>
[CacheName("SufiAI-WorkspaceProviderModelStamp")]
public class WorkspaceProviderModelCacheStamp
{
    public string Stamp { get; set; } = string.Empty;
}
