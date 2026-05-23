using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.FeatureManagement.EntityFrameworkCore;

[DependsOn(
    typeof(SufiAbpFeatureManagementDomainModule),
    typeof(SufiAbpEntityFrameworkCoreModule)
)]
public class SufiAbpFeatureManagementEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<FeatureManagementDbContext>(options =>
        {
            options.AddRepository<FeatureGroupDefinitionRecord, EfCoreFeatureGroupDefinitionRecordRepository>();
            options.AddRepository<FeatureDefinitionRecord, EfCoreFeatureDefinitionRecordRepository>();
            options.AddDefaultRepositories<IFeatureManagementDbContext>();

            options.AddRepository<FeatureValue, EfCoreFeatureValueRepository>();
        });
    }
}
