using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.SettingManagement;

public class SettingManager : ISettingManager, ITransientDependency
{
    protected ISettingDefinitionManager SettingDefinitionManager { get; }
    protected ISettingManagementStore SettingManagementStore { get; }
    protected ISettingProvider SettingProvider { get; }
    protected ICurrentTenant CurrentTenant { get; }

    public SettingManager(
        ISettingDefinitionManager settingDefinitionManager,
        ISettingManagementStore settingManagementStore,
        ISettingProvider settingProvider,
        ICurrentTenant currentTenant)
    {
        SettingDefinitionManager = settingDefinitionManager;
        SettingManagementStore = settingManagementStore;
        SettingProvider = settingProvider;
        CurrentTenant = currentTenant;
    }

    public virtual Task<string?> GetOrNullGlobalAsync(string name)
    {
        return GetOrNullAsync(name, GlobalSettingValueProvider.ProviderName, null, fallback: true);
    }

    public virtual Task<string?> GetOrNullForTenantAsync(string name, Guid tenantId, bool fallback = true)
    {
        return GetOrNullAsync(name, TenantSettingValueProvider.ProviderName, tenantId.ToString(), fallback);
    }

    public virtual async Task<string?> GetOrNullForCurrentTenantAsync(string name, bool fallback = true)
    {
        return CurrentTenant.Id.HasValue
            ? await GetOrNullForTenantAsync(name, CurrentTenant.Id.Value, fallback)
            : await GetOrNullGlobalAsync(name);
    }

    public virtual Task SetGlobalAsync(string name, string? value)
    {
        return SetAsync(name, value, GlobalSettingValueProvider.ProviderName, null);
    }

    public virtual Task SetForTenantAsync(Guid tenantId, string name, string? value)
    {
        return SetAsync(name, value, TenantSettingValueProvider.ProviderName, tenantId.ToString());
    }

    public virtual async Task SetForCurrentTenantAsync(string name, string? value)
    {
        if (!CurrentTenant.Id.HasValue)
        {
            await SetGlobalAsync(name, value);
            return;
        }

        await SetForTenantAsync(CurrentTenant.Id.Value, name, value);
    }

    public virtual Task SetForTenantOrGlobalAsync(Guid? tenantId, string name, string? value)
    {
        return tenantId.HasValue
            ? SetForTenantAsync(tenantId.Value, name, value)
            : SetGlobalAsync(name, value);
    }

    protected virtual async Task<string?> GetOrNullAsync(string name, string providerName, string? providerKey, bool fallback)
    {
        var value = await SettingManagementStore.GetOrNullAsync(name, providerName, providerKey);
        if (!fallback || value != null)
        {
            return value;
        }

        return await SettingProvider.GetOrNullAsync(name);
    }

    protected virtual async Task SetAsync(string name, string? value, string providerName, string? providerKey)
    {
        var setting = await SettingDefinitionManager.GetAsync(name);

        if (value == null)
        {
            await SettingManagementStore.DeleteAsync(setting.Name, providerName, providerKey);
            return;
        }

        await SettingManagementStore.SetAsync(setting.Name, value, providerName, providerKey);
    }
}
