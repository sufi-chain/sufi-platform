using Volo.Abp.Data;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Data;

/// <summary>
/// Configures Sufi Platform data defaults.
/// Table/collection prefixes are now configured in each module's Domain layer.
/// </summary>
[DependsOn(
    typeof(AbpDataModule)
    )]
public class SufiAbpDataModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<SufiAbpDataSeedOptions>(_ => { });
    }
}
