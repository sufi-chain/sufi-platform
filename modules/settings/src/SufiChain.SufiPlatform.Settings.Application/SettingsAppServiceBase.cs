using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Settings.Localization;

namespace SufiChain.SufiPlatform.Settings;

public abstract class SettingsAppServiceBase : SufiApplicationService
{
    protected SettingsAppServiceBase()
    {
        ObjectMapperContext = typeof(SufiSettingsApplicationModule);
        LocalizationResource = typeof(SufiSettingsResource);
    }
}
