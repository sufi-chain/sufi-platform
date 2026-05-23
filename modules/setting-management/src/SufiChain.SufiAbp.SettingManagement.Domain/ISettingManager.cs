namespace SufiChain.SufiAbp.SettingManagement;

public interface ISettingManager
{
    Task<string?> GetOrNullGlobalAsync(string name);

    Task<string?> GetOrNullForTenantAsync(string name, Guid tenantId, bool fallback = true);

    Task<string?> GetOrNullForCurrentTenantAsync(string name, bool fallback = true);

    Task SetGlobalAsync(string name, string? value);

    Task SetForTenantAsync(Guid tenantId, string name, string? value);

    Task SetForCurrentTenantAsync(string name, string? value);

    Task SetForTenantOrGlobalAsync(Guid? tenantId, string name, string? value);
}
