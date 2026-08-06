using System.Threading.Tasks;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Settings;

public abstract class SettingsProvider : ISettingsProvider
{
    public abstract string Name { get; }

    //TODO: Rename to Store
    protected ISettingsStore SettingsStore { get; }

    protected SettingsProvider(ISettingsStore settingManagementStore)
    {
        SettingsStore = settingManagementStore;
    }

    public virtual async Task<string> GetOrNullAsync(SettingDefinition setting, string providerKey)
    {
        return await SettingsStore.GetOrNullAsync(setting.Name, Name, NormalizeProviderKey(providerKey));
    }

    public virtual async Task SetAsync(SettingDefinition setting, string value, string providerKey)
    {
        await SettingsStore.SetAsync(setting.Name, value, Name, NormalizeProviderKey(providerKey));
    }

    public virtual async Task ClearAsync(SettingDefinition setting, string providerKey)
    {
        await SettingsStore.DeleteAsync(setting.Name, Name, NormalizeProviderKey(providerKey));
    }

    protected virtual string NormalizeProviderKey(string providerKey)
    {
        return providerKey;
    }
}
