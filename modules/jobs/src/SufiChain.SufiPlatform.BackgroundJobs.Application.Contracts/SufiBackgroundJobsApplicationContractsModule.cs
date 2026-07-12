using SufiChain.SufiPlatform.Authorization;
using SufiChain.SufiPlatform.Ddd;
using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.BackgroundJobs;

[DependsOn(
    typeof(SufiDddApplicationContractsModule),
    typeof(SufiAuthorizationModule),
    typeof(SufiBackgroundJobsDomainSharedModule)
)]
public class SufiBackgroundJobsApplicationContractsModule : AbpModule
{
}
