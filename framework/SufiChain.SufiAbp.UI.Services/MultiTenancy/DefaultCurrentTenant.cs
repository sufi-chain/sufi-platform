using SufiChain.SufiAbp.UI.MultiTenancy;

namespace SufiChain.SufiAbp.UI.MultiTenancy;

/// <summary>
/// Default no-op implementation of ICurrentTenant.
/// Always returns null (host context). Replace with a product-specific implementation
/// when multi-tenancy is active.
/// </summary>
public class DefaultCurrentTenant : ICurrentTenant
{
    public bool IsAvailable => false;

    public Guid? Id => null;

    public string? Name => null;

    public IDisposable Change(Guid? id, string? name = null)
    {
        return NullDisposable.Instance;
    }

    private sealed class NullDisposable : IDisposable
    {
        public static readonly NullDisposable Instance = new();
        public void Dispose() { }
    }
}
