using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Null fallback used when no AI provider module is installed: no workspaces exist.
/// </summary>
[Dependency(TryRegister = true)]
[ExposeServices(typeof(ISufiAbpAIWorkspaceCatalog))]
public class NullSufiAbpAIWorkspaceCatalog : ISufiAbpAIWorkspaceCatalog, ITransientDependency
{
    /// <inheritdoc />
    public virtual Task<List<SufiAbpAIWorkspaceDescriptor>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<SufiAbpAIWorkspaceDescriptor>());
    }

    /// <inheritdoc />
    public virtual Task<SufiAbpAIWorkspaceDescriptor?> FindAsync(string name, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<SufiAbpAIWorkspaceDescriptor?>(null);
    }
}

/// <summary>
/// Null fallback used when no AI provider module is installed: nothing resolves.
/// </summary>
[Dependency(TryRegister = true)]
[ExposeServices(typeof(ISufiAbpAIWorkspaceResolver))]
public class NullSufiAbpAIWorkspaceResolver : ISufiAbpAIWorkspaceResolver, ITransientDependency
{
    /// <inheritdoc />
    public virtual Task<SufiAbpAIWorkspaceDescriptor?> ResolveAsync(
        string? preferredWorkspaceName = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<SufiAbpAIWorkspaceDescriptor?>(null);
    }
}
