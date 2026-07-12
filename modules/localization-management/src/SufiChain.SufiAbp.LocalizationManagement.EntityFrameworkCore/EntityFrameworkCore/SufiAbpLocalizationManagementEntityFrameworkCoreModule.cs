using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.LocalizationManagement.Entities;
using SufiChain.SufiAbp.LocalizationManagement.Repositories;
using Volo.Abp.Modularity;

using Volo.Abp.EntityFrameworkCore;
namespace SufiChain.SufiAbp.LocalizationManagement.EntityFrameworkCore;

[DependsOn(
    typeof(SufiAbpLocalizationManagementDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
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
