using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Localization.Entities;
using SufiChain.SufiPlatform.Localization.Repositories;
using SufiChain.SufiPlatform.Localization;
using Volo.Abp.Data;
using Volo.Abp.Modularity;

using Volo.Abp.EntityFrameworkCore;
namespace SufiChain.SufiPlatform.Localization.EntityFrameworkCore;

[DependsOn(
    typeof(SufiLocalizationDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class SufiLocalizationEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<LocalizationDbContext>(options =>
        {
            options.AddDefaultRepositories<ISufiLocalizationDbContext>(includeAllEntities: true);

            options.AddRepository<LocalizationResource, EfCoreLocalizationResourceRepository>();
            options.AddRepository<LocalizationText, EfCoreLocalizationTextRepository>();
        });

        Configure<AbpDbConnectionOptions>(options =>
        {
            options.Databases.Configure(SufiLocalizationDbProperties.ConnectionStringName, database =>
            {
                database.IsUsedByTenants = true;
            });
        });
    }
}
