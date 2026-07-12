using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Settings;

/// <summary>
/// Bridges <see cref="ISettingsStore"/> to <see cref="ISettingStore"/> so
/// <see cref="ISettingProvider"/> reads values saved from Setting Management UI.
/// </summary>
public class SettingStore : ISettingStore, ITransientDependency
{
    protected ISettingsStore ManagementStore { get; }

    public SettingStore(ISettingsStore managementStore)
    {
        ManagementStore = managementStore;
    }

    public virtual Task<string?> GetOrNullAsync(string name, string? providerName, string? providerKey)
    {
        return ManagementStore.GetOrNullAsync(name, providerName!, providerKey!);
    }

    public virtual Task<List<SettingValue>> GetAllAsync(string[] names, string? providerName, string? providerKey)
    {
        return ManagementStore.GetListAsync(names, providerName!, providerKey!);
    }
}
