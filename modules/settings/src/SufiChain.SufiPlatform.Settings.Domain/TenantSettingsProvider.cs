using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Settings;

public class TenantSettingsProvider : SettingsProvider, ITransientDependency
{
    public override string Name => TenantSettingValueProvider.ProviderName;

    protected ICurrentTenant CurrentTenant { get; }

    public TenantSettingsProvider(
        ISettingsStore settingManagementStore,
        ICurrentTenant currentTenant)
        : base(settingManagementStore)
    {
        CurrentTenant = currentTenant;
    }

    protected override string NormalizeProviderKey(string providerKey)
    {
        if (providerKey != null)
        {
            return providerKey;
        }

        return CurrentTenant.Id?.ToString();
    }
}
