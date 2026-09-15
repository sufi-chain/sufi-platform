using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Features;
using Volo.Abp.Modularity;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SufiCom.Application.Tests.Connections;

/// <summary>
/// Test module that enables the SufiCom Telegram features so <c>[RequiresFeature]</c> on
/// <c>TelegramConnectionAppService</c> passes during tests. Mirrors the Chat test pattern.
/// </summary>
[DependsOn(typeof(SufiComApplicationTestModule))]
public class SufiComTelegramApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<TelegramTestFeatureChecker>();
        context.Services.AddSingleton<IFeatureChecker>(sp => sp.GetRequiredService<TelegramTestFeatureChecker>());
    }
}

internal sealed class TelegramTestFeatureChecker : IFeatureChecker
{
    public Task<string?> GetOrNullAsync(string name) => Task.FromResult<string?>(null);

    public Task<bool> IsEnabledAsync(string name) => Task.FromResult(true);
}
