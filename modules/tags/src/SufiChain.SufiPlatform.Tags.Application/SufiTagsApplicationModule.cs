using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Tags;

[DependsOn(
    typeof(SufiTagsDomainModule),
    typeof(SufiTagsApplicationContractsModule),
    typeof(AbpMapperlyModule)
)]
public class SufiTagsApplicationModule : AbpModule
{
}