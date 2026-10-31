using System;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.Settings;

public class SettingManagerProviderTests
{
    [Fact]
    public async Task Host_only_setting_accepts_global_storage_and_rejects_tenant_and_user()
    {
        var definition = new SettingDefinition("SufiFinance.AiCredit.NegativeGrace", "2.00")
            .RestrictToHost();
        var manager = Create(definition, out var store, out _);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await manager.SetGlobalAsync(definition.Name, "3.00");
        await store.Received(1).SetAsync(
            definition.Name,
            "3.00",
            GlobalSettingValueProvider.ProviderName,
            null);

        await Should.ThrowAsync<AbpException>(() => manager.SetForTenantAsync(tenantId, definition.Name, "1"));
        await Should.ThrowAsync<AbpException>(() => manager.SetForUserAsync(userId, definition.Name, "1"));
        await Should.ThrowAsync<AbpException>(() => manager.SetForTenantAsync(tenantId, definition.Name, null));
        await store.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Empty_provider_list_still_allows_user_and_tenant_storage()
    {
        var definition = new SettingDefinition("Abp.Localization.DefaultLanguage", "en");
        var manager = Create(definition, out var store, out _);
        var userId = Guid.NewGuid();

        await manager.SetForUserAsync(userId, definition.Name, "fa");

        await store.Received(1).SetAsync(
            definition.Name,
            "fa",
            UserSettingValueProvider.ProviderName,
            userId.ToString());
    }

    [Fact]
    public async Task Global_and_tenant_setting_rejects_user_storage()
    {
        var definition = new SettingDefinition("SufiAI.WebResearch.Enabled", "false");
        definition.Providers.Add(GlobalSettingValueProvider.ProviderName);
        definition.Providers.Add(TenantSettingValueProvider.ProviderName);
        var manager = Create(definition, out var store, out _);

        await manager.SetForTenantAsync(Guid.NewGuid(), definition.Name, "true");
        await Should.ThrowAsync<AbpException>(() => manager.SetForUserAsync(Guid.NewGuid(), definition.Name, "true"));
        await store.DidNotReceive().SetAsync(
            definition.Name,
            "true",
            UserSettingValueProvider.ProviderName,
            Arg.Any<string>());
    }

    [Fact]
    public async Task Disallowed_provider_read_uses_fallback_or_returns_null()
    {
        var definition = new SettingDefinition("SufiFinance.AiCredit.NegativeGrace", "2.00")
            .RestrictToHost();
        var manager = Create(definition, out var store, out var provider);
        provider.GetOrNullAsync(definition.Name).Returns("2.00");

        var resolved = await manager.GetOrNullForTenantAsync(definition.Name, Guid.NewGuid(), fallback: true);
        var direct = await manager.GetOrNullForTenantAsync(definition.Name, Guid.NewGuid(), fallback: false);

        resolved.ShouldBe("2.00");
        direct.ShouldBeNull();
        await store.DidNotReceive().GetOrNullAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void Restrict_to_host_keeps_default_configuration_and_global_providers()
    {
        var definition = new SettingDefinition("Host.Only", "1");
        definition.Providers.Add(TenantSettingValueProvider.ProviderName);
        definition.Providers.Add(UserSettingValueProvider.ProviderName);

        definition.RestrictToHost();

        definition.Providers.ShouldContain(DefaultValueSettingValueProvider.ProviderName);
        definition.Providers.ShouldContain(ConfigurationSettingValueProvider.ProviderName);
        definition.Providers.ShouldContain(GlobalSettingValueProvider.ProviderName);
        definition.Providers.ShouldNotContain(TenantSettingValueProvider.ProviderName);
        definition.Providers.ShouldNotContain(UserSettingValueProvider.ProviderName);
    }

    private static SettingManager Create(
        SettingDefinition definition,
        out ISettingsStore store,
        out ISettingProvider provider)
    {
        store = Substitute.For<ISettingsStore>();
        provider = Substitute.For<ISettingProvider>();
        var definitions = Substitute.For<ISettingDefinitionManager>();
        definitions.GetAsync(definition.Name).Returns(definition);

        return new SettingManager(
            definitions,
            Substitute.For<ISettingEncryptionService>(),
            store,
            provider,
            Substitute.For<ICurrentTenant>());
    }
}
