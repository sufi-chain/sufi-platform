using SufiChain.SufiPlatform.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Tags;

[DependsOn(
    typeof(SufiTagsApplicationContractsModule),
    typeof(SufiAspNetCoreMvcModule)
)]
public class SufiTagsHttpApiModule : AbpModule
{
}