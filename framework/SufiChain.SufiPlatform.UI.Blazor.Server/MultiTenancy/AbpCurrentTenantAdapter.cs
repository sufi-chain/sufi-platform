using SufiUiCurrentTenant = SufiChain.SufiPlatform.UI.MultiTenancy.ICurrentTenant;
using AbpCurrentTenant = Volo.Abp.MultiTenancy.ICurrentTenant;

namespace SufiChain.SufiPlatform.UI.Blazor.Server.MultiTenancy;

/// <summary>
/// Bridges ABP <see cref="Volo.Abp.MultiTenancy.ICurrentTenant"/> into the Sufi UI tenant contract.
/// <c>DefaultCurrentTenant</c> is a no-op and always reports host; without this adapter,
/// switch-tenant UI stays on host even when the <c>__tenant</c> cookie has already selected a tenant.
/// </summary>
public class AbpCurrentTenantAdapter : SufiUiCurrentTenant
{
    private readonly AbpCurrentTenant _currentTenant;

    public AbpCurrentTenantAdapter(AbpCurrentTenant currentTenant)
    {
        _currentTenant = currentTenant;
    }

    public bool IsAvailable => _currentTenant.IsAvailable;

    public Guid? Id => _currentTenant.Id;

    public string? Name => _currentTenant.Name;

    public IDisposable Change(Guid? id, string? name = null)
    {
        return _currentTenant.Change(id, name);
    }
}
