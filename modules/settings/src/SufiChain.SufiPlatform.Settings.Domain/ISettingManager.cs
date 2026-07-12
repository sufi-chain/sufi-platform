namespace SufiChain.SufiPlatform.Settings;

public interface ISettingManager
{
    /// <summary>
    /// Gets a setting for the given tenant, or at host level when <paramref name="tenantId"/> is null.
    /// </summary>
    Task<string?> GetOrNullAsync(string name, Guid? tenantId = null, bool fallback = true);

    /// <summary>
    /// Sets a setting for the given tenant, or at host level when <paramref name="tenantId"/> is null.
    /// </summary>
    Task SetAsync(string name, string? value, Guid? tenantId = null);

    Task<string?> GetOrNullGlobalAsync(string name);

    Task<string?> GetOrNullForTenantAsync(string name, Guid tenantId, bool fallback = true);

    Task<string?> GetOrNullForCurrentTenantAsync(string name, bool fallback = true);

    Task SetGlobalAsync(string name, string? value);

    Task SetForTenantAsync(Guid tenantId, string name, string? value);

    Task SetForCurrentTenantAsync(string name, string? value);

    Task SetForTenantOrGlobalAsync(Guid? tenantId, string name, string? value);
}
