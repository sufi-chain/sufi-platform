using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Lists and looks up AI workspaces as credential-free descriptors.
/// Lets product modules enumerate/validate workspaces without referencing
/// provider repositories or reading credentials.
/// </summary>
public interface ISufiAIWorkspaceCatalog
{
    /// <summary>
    /// Gets all workspaces visible to the current tenant.
    /// Returns an empty list when no provider module is installed.
    /// </summary>
    Task<List<SufiAIWorkspaceDescriptor>> GetListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a workspace by its unique name. Returns <c>null</c> when not found
    /// or when no provider module is installed.
    /// </summary>
    Task<SufiAIWorkspaceDescriptor?> FindAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a workspace by its identifier. Returns <c>null</c> when not found
    /// or when no provider module is installed.
    /// </summary>
    Task<SufiAIWorkspaceDescriptor?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
