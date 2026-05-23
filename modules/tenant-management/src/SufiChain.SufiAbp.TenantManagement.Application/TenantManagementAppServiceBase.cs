using SufiChain.SufiAbp.TenantManagement.Localization;
using SufiChain.SufiAbp.Application.Services;

namespace SufiChain.SufiAbp.TenantManagement;

public abstract class TenantManagementAppServiceBase : SufiAbpApplicationService
{
    protected TenantManagementAppServiceBase()
    {
        ObjectMapperContext = typeof(SufiAbpTenantManagementApplicationModule);
        LocalizationResource = typeof(SufiAbpTenantManagementResource);
    }
}
