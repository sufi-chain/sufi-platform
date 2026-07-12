using SufiChain.SufiPlatform.Tenants.Localization;
using SufiChain.SufiPlatform.UI.Blazor;

namespace SufiChain.SufiPlatform.Tenants.Blazor;

/// <summary>
/// Base class for Blazor components in the Tenant Management module.
/// Provides module localization via TenantsResource.
/// </summary>
public abstract class TenantsComponentBase : SufiComponentBase
{
    protected TenantsComponentBase()
    {
        LocalizationResource = typeof(SufiTenantsResource);
    }
}
