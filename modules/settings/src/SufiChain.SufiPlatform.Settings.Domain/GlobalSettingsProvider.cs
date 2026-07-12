using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Settings;

public class GlobalSettingsProvider : SettingsProvider, ITransientDependency
{
    public override string Name => GlobalSettingValueProvider.ProviderName;

    public GlobalSettingsProvider(ISettingsStore settingManagementStore)
        : base(settingManagementStore)
    {

    }

    protected override string NormalizeProviderKey(string providerKey)
    {
        return null;
    }
}
