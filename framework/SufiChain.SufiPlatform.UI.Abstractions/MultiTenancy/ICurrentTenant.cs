namespace SufiChain.SufiPlatform.UI.MultiTenancy;

/// <summary>
/// Provides access to the current tenant information.
/// </summary>
public interface ICurrentTenant
{
    /// <summary>
    /// Whether a tenant is available/active.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// The current tenant's ID, or null if no tenant.
    /// </summary>
    Guid? Id { get; }

    /// <summary>
    /// The current tenant's name, or null if no tenant.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Changes the current tenant context temporarily.
    /// Dispose the returned object to restore the previous tenant.
    /// </summary>
    /// <param name="id">The tenant ID to change to, or null for host.</param>
    /// <param name="name">Optional tenant name.</param>
    /// <returns>A disposable that restores the previous tenant when disposed.</returns>
    IDisposable Change(Guid? id, string? name = null);
}
