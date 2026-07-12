using SufiChain.SufiPlatform.Application.Localization.Resources.SufiDdd;
using Volo.Abp.Application;
using Volo.Abp.Localization;
using Volo.Abp.VirtualFileSystem;
using Volo.Abp.Modularity;

using Volo.Abp.Domain;
namespace SufiChain.SufiPlatform.Ddd;

[DependsOn(
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpDddDomainSharedModule)
)]
public class SufiDddApplicationContractsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiDddApplicationContractsModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiDddApplicationContractsResource>("en")
                .AddVirtualJson("/Localization/SufiDddApplicationContracts");
        });
    }
}
