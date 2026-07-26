using System.Data;

namespace SufiChain.SufiPlatform.OpenIddict;

/// <summary>
/// Isolation-level options for OpenIddict store prune/delete operations.
/// </summary>
public class OpenIddictStoreOptions
{
    public IsolationLevel? PruneIsolationLevel { get; set; }

    public IsolationLevel? DeleteIsolationLevel { get; set; }

    public OpenIddictStoreOptions()
    {
        PruneIsolationLevel = IsolationLevel.RepeatableRead;
        DeleteIsolationLevel = IsolationLevel.Serializable;
    }
}
