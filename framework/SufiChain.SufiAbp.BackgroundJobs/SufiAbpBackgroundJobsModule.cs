using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.BackgroundJobs;

[DependsOn(
    typeof(AbpBackgroundJobsModule)
)]
public class SufiAbpBackgroundJobsModule : AbpModule
{
}
