using Volo.Abp.Data;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Data;

/// <summary>
/// Configures Sufi Platform data defaults.
/// Table/collection prefixes are now configured in each module's Domain layer.
/// Common ABP event tables use <see cref="SufiCommonDbProperties"/>
/// (<c>SufiEvents.Inbox</c> / <c>SufiEvents.Outbox</c>).
/// </summary>
[DependsOn(
    typeof(AbpDataModule)
    )]
public class SufiDataModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<SufiDataSeedOptions>(_ => { });
        SufiCommonDbProperties.ApplyToAbp();
    }
}
