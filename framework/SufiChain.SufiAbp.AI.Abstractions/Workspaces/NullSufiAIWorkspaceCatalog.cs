using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Null fallback used when no AI provider module is installed: no workspaces exist.
/// </summary>
[Dependency(TryRegister = true)]
[ExposeServices(typeof(ISufiAIWorkspaceCatalog))]
public class NullSufiAIWorkspaceCatalog : ISufiAIWorkspaceCatalog, ITransientDependency
{
    /// <inheritdoc />
    public virtual Task<List<SufiAIWorkspaceDescriptor>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<SufiAIWorkspaceDescriptor>());
    }

    /// <inheritdoc />
    public virtual Task<SufiAIWorkspaceDescriptor?> FindAsync(string name, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<SufiAIWorkspaceDescriptor?>(null);
    }

    /// <inheritdoc />
    public virtual Task<SufiAIWorkspaceDescriptor?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<SufiAIWorkspaceDescriptor?>(null);
    }
}

/// <summary>
/// Null fallback used when no AI provider module is installed: nothing resolves.
/// </summary>
[Dependency(TryRegister = true)]
[ExposeServices(typeof(ISufiAIWorkspaceResolver))]
public class NullSufiAIWorkspaceResolver : ISufiAIWorkspaceResolver, ITransientDependency
{
    /// <inheritdoc />
    public virtual Task<SufiAIWorkspaceDescriptor?> ResolveAsync(
        string? preferredWorkspaceName = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<SufiAIWorkspaceDescriptor?>(null);
    }
}
