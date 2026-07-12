using SufiChain.SufiPlatform.Tenants.Localization;
using SufiChain.SufiPlatform.Application.Services;

namespace SufiChain.SufiPlatform.Tenants;

public abstract class TenantsAppServiceBase : SufiApplicationService
{
    protected TenantsAppServiceBase()
    {
        ObjectMapperContext = typeof(SufiTenantsApplicationModule);
        LocalizationResource = typeof(SufiTenantsResource);
    }
}
