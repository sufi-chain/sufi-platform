using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.SettingManagement;

/// <summary>
/// Bridges <see cref="ISettingManagementStore"/> to <see cref="ISettingStore"/> so
/// <see cref="ISettingProvider"/> reads values saved from Setting Management UI.
/// </summary>
public class SettingStore : ISettingStore, ITransientDependency
{
    protected ISettingManagementStore ManagementStore { get; }

    public SettingStore(ISettingManagementStore managementStore)
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
