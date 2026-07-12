using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.SufiAI.Configuration;
using SufiChain.SufiPlatform.Ddd;
using SufiChain.SufiPlatform.FileManager.Configuration;
using SufiChain.SufiPlatform.Localization;
using Volo.Abp.Modularity;

using Volo.Abp.Mapperly;
namespace SufiChain.SufiPlatform.SufiAI;

[DependsOn(
    typeof(SufiAIDomainModule),
    typeof(SufiAIApplicationContractsModule),
    typeof(SufiLocalizationApplicationContractsModule),
    typeof(SufiDddApplicationModule),
    typeof(AbpMapperlyModule)
)]
public class SufiAIApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        Configure<AIOptions>(configuration.GetSection("SufiAI"));

        Configure<FileManagerOptions>(options => new AIOptions().AddDefaultFileStructure(options));

        context.Services.AddMapperlyObjectMapper<SufiAIApplicationModule>();
    }
}
