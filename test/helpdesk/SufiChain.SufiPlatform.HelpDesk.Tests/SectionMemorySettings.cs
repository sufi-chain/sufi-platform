using SufiChain.SufiPlatform.Settings;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.HelpDesk;

internal sealed class SectionMemorySettings : ISettingProvider, ISettingManager
{
    private readonly Dictionary<string, string?> _values = new(StringComparer.Ordinal);

    public string? Get(string name) => _values.TryGetValue(name, out var value) ? value : null;

    public Task<string?> GetOrNullAsync(string name) => Task.FromResult(Get(name));

    public Task<List<SettingValue>> GetAllAsync(string[] names) =>
        Task.FromResult(names.Select(name => new SettingValue(name, Get(name) ?? string.Empty)).ToList());

    public Task<List<SettingValue>> GetAllAsync() =>
        Task.FromResult(_values.Select(pair => new SettingValue(pair.Key, pair.Value ?? string.Empty)).ToList());

    public Task<string?> GetOrNullAsync(string name, Guid? tenantId = null, bool fallback = true) => GetOrNullAsync(name);

    public Task SetAsync(string name, string? value, Guid? tenantId = null)
    {
        _values[name] = value;
        return Task.CompletedTask;
    }

    public Task<string?> GetOrNullGlobalAsync(string name) => GetOrNullAsync(name);
    public Task<string?> GetOrNullForTenantAsync(string name, Guid tenantId, bool fallback = true) => GetOrNullAsync(name);
    public Task<string?> GetOrNullForCurrentTenantAsync(string name, bool fallback = true) => GetOrNullAsync(name);
    public Task<string?> GetOrNullForUserAsync(Guid userId, string name, bool fallback = true) => GetOrNullAsync(name);
    public Task SetGlobalAsync(string name, string? value) => SetAsync(name, value);
    public Task SetForTenantAsync(Guid tenantId, string name, string? value) => SetAsync(name, value);
    public Task SetForUserAsync(Guid userId, string name, string? value) => SetAsync(name, value);
    public Task SetForCurrentTenantAsync(string name, string? value) => SetAsync(name, value);
    public Task SetForTenantOrGlobalAsync(Guid? tenantId, string name, string? value) => SetAsync(name, value);
}
