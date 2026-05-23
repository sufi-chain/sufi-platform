using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.LocalizationManagement.MongoDB;
using SufiChain.SufiAbp.MongoDB;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.LocalizationManagement;

[DependsOn(
    typeof(SufiAbpLocalizationManagementDomainModule),
    typeof(SufiAbpMongoDbModule)
)]
public class SufiAbpLocalizationManagementMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<LocalizationManagementMongoDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });
    }
}
