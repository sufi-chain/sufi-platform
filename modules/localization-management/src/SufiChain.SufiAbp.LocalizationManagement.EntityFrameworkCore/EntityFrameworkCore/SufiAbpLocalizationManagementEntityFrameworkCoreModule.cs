using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.EntityFrameworkCore;
using SufiChain.SufiAbp.LocalizationManagement.Entities;
using SufiChain.SufiAbp.LocalizationManagement.Repositories;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.LocalizationManagement.EntityFrameworkCore;

[DependsOn(
    typeof(SufiAbpLocalizationManagementDomainModule),
    typeof(SufiAbpEntityFrameworkCoreModule)
)]
public class SufiAbpLocalizationManagementEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<LocalizationManagementDbContext>(options =>
        {
            options.AddDefaultRepositories<ISufiAbpLocalizationManagementDbContext>(includeAllEntities: true);

            options.AddRepository<LocalizationResource, EfCoreLocalizationResourceRepository>();
            options.AddRepository<LocalizationText, EfCoreLocalizationTextRepository>();
        });
    }
}
