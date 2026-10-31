using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.SufiCom.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
using Volo.Abp.Validation;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom;

public class SufiComSettingsSectionTests
{
    [Fact]
    public async Task Zero_retention_and_timeout_are_stored()
    {
        var store = new SectionMemorySettings();
        var service = Host(new SufiComSettingsAppService(store, store));

        await service.UpdateAsync(new SufiComSettingsDto { LogRetentionDays = 0, DefaultTimeoutSeconds = 0 });

        store.Get(SufiComSettings.LogRetentionDays).ShouldBe("0");
        store.Get(SufiComSettings.DefaultTimeoutSeconds).ShouldBe("0");
    }

    [Fact]
    public async Task A_timeout_over_one_hour_is_rejected()
    {
        var store = new SectionMemorySettings();
        var service = Host(new SufiComSettingsAppService(store, store));

        await Should.ThrowAsync<AbpValidationException>(() => service.UpdateAsync(new SufiComSettingsDto
        {
            LogRetentionDays = 90,
            DefaultTimeoutSeconds = 3601
        }));
    }

    private static T Host<T>(T service) where T : Volo.Abp.Application.Services.ApplicationService
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICurrentTenant, HostTenant>();
        service.LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider());
        return service;
    }

    private sealed class HostTenant : ICurrentTenant
    {
        public bool IsAvailable => false;
        public Guid? Id => null;
        public string? Name => null;
        public IDisposable Change(Guid? id, string? name = null) => new Noop();
        private sealed class Noop : IDisposable { public void Dispose() { } }
    }

    private sealed class SectionMemorySettings : ISettingProvider, ISettingManager
    {
        private readonly Dictionary<string, string?> _values = new(StringComparer.Ordinal);
        public string? Get(string name) => _values.TryGetValue(name, out var value) ? value : null;
        public Task<string?> GetOrNullAsync(string name) => Task.FromResult(Get(name));
        public Task<List<SettingValue>> GetAllAsync(string[] names) => Task.FromResult(names.Select(name => new SettingValue(name, Get(name) ?? string.Empty)).ToList());
        public Task<List<SettingValue>> GetAllAsync() => Task.FromResult(_values.Select(pair => new SettingValue(pair.Key, pair.Value ?? string.Empty)).ToList());
        public Task<string?> GetOrNullAsync(string name, Guid? tenantId = null, bool fallback = true) => GetOrNullAsync(name);
        public Task SetAsync(string name, string? value, Guid? tenantId = null) { _values[name] = value; return Task.CompletedTask; }
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
}
