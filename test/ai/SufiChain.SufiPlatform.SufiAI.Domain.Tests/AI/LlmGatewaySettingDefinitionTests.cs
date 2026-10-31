using Shouldly;
using Volo.Abp.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.AI;

public class LlmGatewaySettingDefinitionTests
{
    [Fact]
    public void Gateway_limits_are_stored_only_at_host_scope()
    {
        var settings = new Dictionary<string, SettingDefinition>();
        var contextType = typeof(SettingDefinition).Assembly.GetType("Volo.Abp.Settings.SettingDefinitionContext");
        Assert.NotNull(contextType);
        var context = (ISettingDefinitionContext)Activator.CreateInstance(contextType!, settings)!;
        new LlmGatewaySettingDefinitionProvider().Define(context);

        settings.Count.ShouldBe(10);
        foreach (var setting in settings.Values)
        {
            setting.Providers.ShouldContain(GlobalSettingValueProvider.ProviderName);
            setting.Providers.ShouldNotContain(TenantSettingValueProvider.ProviderName);
            setting.Providers.ShouldNotContain(UserSettingValueProvider.ProviderName);
        }
    }
}
