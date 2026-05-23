using SufiChain.SufiAbp.TenantManagement.Localization;
using SufiChain.SufiAbp.UI.Blazor;

namespace SufiChain.SufiAbp.TenantManagement.Blazor;

/// <summary>
/// Base class for Blazor components in the Tenant Management module.
/// Provides module localization via TenantManagementResource.
/// </summary>
public abstract class TenantManagementComponentBase : SufiAbpComponentBase
{
    protected TenantManagementComponentBase()
    {
        LocalizationResource = typeof(SufiAbpTenantManagementResource);
    }
}
