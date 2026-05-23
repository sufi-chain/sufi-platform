using SufiChain.SufiAbp.Application.Localization.Resources.SufiAbpDdd;
using Volo.Abp.Application;
using Volo.Abp.Localization;
using Volo.Abp.VirtualFileSystem;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Ddd;

[DependsOn(
    typeof(AbpDddApplicationContractsModule),
    typeof(SufiAbpDddDomainSharedModule)
)]
public class SufiAbpDddApplicationContractsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpDddApplicationContractsModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiAbpDddApplicationContractsResource>("en")
                .AddVirtualJson("/Localization/SufiAbpDddApplicationContracts");
        });
    }
}
