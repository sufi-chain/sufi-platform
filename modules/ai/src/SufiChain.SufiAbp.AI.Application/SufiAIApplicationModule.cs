using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.AI.Configuration;
using SufiChain.SufiAbp.Ddd;
using SufiChain.SufiAbp.FileManager.Configuration;
using SufiChain.SufiAbp.LocalizationManagement;
using Volo.Abp.Modularity;

using Volo.Abp.Mapperly;
namespace SufiChain.SufiAbp.AI;

[DependsOn(
    typeof(SufiAIDomainModule),
    typeof(SufiAIApplicationContractsModule),
    typeof(SufiAbpLocalizationManagementApplicationContractsModule),
    typeof(SufiAbpDddApplicationModule),
    typeof(AbpMapperlyModule)
)]
public class SufiAIApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        Configure<AIOptions>(configuration.GetSection("AI"));

        Configure<FileManagerOptions>(options => new AIOptions().AddDefaultFileStructure(options));

        context.Services.AddMapperlyObjectMapper<SufiAIApplicationModule>();
    }
}
