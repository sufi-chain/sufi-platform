using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Tags;

[DependsOn(
    typeof(SufiTagsApplicationContractsModule),
    typeof(AbpHttpClientModule)
)]
public class SufiTagsHttpApiClientModule : AbpModule
{
}