using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.LocalizationManagement.MongoDB;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.LocalizationManagement;

[DependsOn(
    typeof(SufiAbpLocalizationManagementDomainModule),
    typeof(AbpMongoDbModule)
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
