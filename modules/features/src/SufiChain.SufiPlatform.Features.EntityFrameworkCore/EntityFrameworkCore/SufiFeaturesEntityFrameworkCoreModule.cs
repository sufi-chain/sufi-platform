using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Features.EntityFrameworkCore;

[DependsOn(
    typeof(SufiFeaturesDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class SufiFeaturesEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<FeaturesDbContext>(options =>
        {
            options.AddRepository<FeatureGroupDefinitionRecord, EfCoreFeatureGroupDefinitionRecordRepository>();
            options.AddRepository<FeatureDefinitionRecord, EfCoreFeatureDefinitionRecordRepository>();
            options.AddDefaultRepositories<IFeaturesDbContext>();

            options.AddRepository<FeatureValue, EfCoreFeatureValueRepository>();
        });
    }
}
