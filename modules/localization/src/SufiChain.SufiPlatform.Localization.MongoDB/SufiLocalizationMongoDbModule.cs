using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Localization.MongoDB;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Localization;

[DependsOn(
    typeof(SufiLocalizationDomainModule),
    typeof(AbpMongoDbModule)
)]
public class SufiLocalizationMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<LocalizationMongoDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });
    }
}
