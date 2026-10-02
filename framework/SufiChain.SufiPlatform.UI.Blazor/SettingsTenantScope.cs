namespace SufiChain.SufiPlatform.UI.Blazor;

/// <summary>
/// Cascades the tenant whose settings a host admin is editing.
/// <see cref="SufiComponentBase"/> enters <c>ICurrentTenant.Change</c> for this id
/// around cascaded load and save calls.
/// </summary>
public sealed class SettingsTenantScope
{
    public SettingsTenantScope(Guid tenantId)
    {
        TenantId = tenantId;
    }

    public Guid TenantId { get; }
}
