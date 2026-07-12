using SufiChain.SufiPlatform.Authorization;
using SufiChain.SufiPlatform.Ddd;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Tags;

[DependsOn(
    typeof(SufiTagsDomainSharedModule),
    typeof(SufiDddApplicationContractsModule),
    typeof(SufiAuthorizationModule)
)]
public class SufiTagsApplicationContractsModule : AbpModule
{
}