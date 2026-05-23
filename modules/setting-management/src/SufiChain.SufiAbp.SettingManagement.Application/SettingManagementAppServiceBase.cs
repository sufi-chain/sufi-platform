using SufiChain.SufiAbp.Application.Services;
using SufiChain.SufiAbp.SettingManagement.Localization;

namespace SufiChain.SufiAbp.SettingManagement;

public abstract class SettingManagementAppServiceBase : SufiAbpApplicationService
{
    protected SettingManagementAppServiceBase()
    {
        ObjectMapperContext = typeof(SufiAbpSettingManagementApplicationModule);
        LocalizationResource = typeof(SufiAbpSettingManagementResource);
    }
}
