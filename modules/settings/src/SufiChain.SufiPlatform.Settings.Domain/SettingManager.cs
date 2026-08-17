using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Settings;

public class SettingManager : ISettingManager, ITransientDependency
{
    protected ISettingDefinitionManager SettingDefinitionManager { get; }
    protected ISettingEncryptionService SettingEncryptionService { get; }
    protected ISettingsStore SettingsStore { get; }
    protected ISettingProvider SettingProvider { get; }
    protected ICurrentTenant CurrentTenant { get; }

    public SettingManager(
        ISettingDefinitionManager settingDefinitionManager,
        ISettingEncryptionService settingEncryptionService,
        ISettingsStore settingManagementStore,
        ISettingProvider settingProvider,
        ICurrentTenant currentTenant)
    {
        SettingDefinitionManager = settingDefinitionManager;
        SettingEncryptionService = settingEncryptionService;
        SettingsStore = settingManagementStore;
        SettingProvider = settingProvider;
        CurrentTenant = currentTenant;
    }

    public virtual Task<string?> GetOrNullAsync(string name, Guid? tenantId = null, bool fallback = true)
    {
        return tenantId.HasValue
            ? GetOrNullForTenantAsync(name, tenantId.Value, fallback)
            : GetOrNullInternalAsync(name, GlobalSettingValueProvider.ProviderName, null, fallback);
    }

    public virtual Task SetAsync(string name, string? value, Guid? tenantId = null)
    {
        return SetForTenantOrGlobalAsync(tenantId, name, value);
    }

    public virtual Task<string?> GetOrNullGlobalAsync(string name)
    {
        return GetOrNullInternalAsync(name, GlobalSettingValueProvider.ProviderName, null, fallback: true);
    }

    public virtual Task<string?> GetOrNullForTenantAsync(string name, Guid tenantId, bool fallback = true)
    {
        return GetOrNullInternalAsync(name, TenantSettingValueProvider.ProviderName, tenantId.ToString(), fallback);
    }

    public virtual async Task<string?> GetOrNullForCurrentTenantAsync(string name, bool fallback = true)
    {
        return CurrentTenant.Id.HasValue
            ? await GetOrNullForTenantAsync(name, CurrentTenant.Id.Value, fallback)
            : await GetOrNullGlobalAsync(name);
    }

    public virtual Task<string?> GetOrNullForUserAsync(Guid userId, string name, bool fallback = true)
    {
        return GetOrNullInternalAsync(name, UserSettingValueProvider.ProviderName, userId.ToString(), fallback);
    }

    public virtual Task SetGlobalAsync(string name, string? value)
    {
        return SetInternalAsync(name, value, GlobalSettingValueProvider.ProviderName, null);
    }

    public virtual Task SetForTenantAsync(Guid tenantId, string name, string? value)
    {
        return SetInternalAsync(name, value, TenantSettingValueProvider.ProviderName, tenantId.ToString());
    }

    public virtual Task SetForUserAsync(Guid userId, string name, string? value)
    {
        return SetInternalAsync(name, value, UserSettingValueProvider.ProviderName, userId.ToString());
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

    protected virtual async Task<string?> GetOrNullInternalAsync(string name, string providerName, string? providerKey, bool fallback)
    {
        var setting = await SettingDefinitionManager.GetAsync(name);
        var value = await SettingsStore.GetOrNullAsync(name, providerName, providerKey);
        if (!fallback || value != null)
        {
            return setting.IsEncrypted
                ? SettingEncryptionService.Decrypt(setting, value)
                : value;
        }

        return await SettingProvider.GetOrNullAsync(name);
    }

    protected virtual async Task SetInternalAsync(string name, string? value, string providerName, string? providerKey)
    {
        var setting = await SettingDefinitionManager.GetAsync(name);

        if (value == null)
        {
            await SettingsStore.DeleteAsync(setting.Name, providerName, providerKey);
            return;
        }

        if (setting.IsEncrypted)
        {
            value = SettingEncryptionService.Encrypt(setting, value)!;
        }

        await SettingsStore.SetAsync(setting.Name, value, providerName, providerKey);
    }
}
