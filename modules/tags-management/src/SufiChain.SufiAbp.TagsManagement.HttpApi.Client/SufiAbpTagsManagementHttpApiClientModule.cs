using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.TagsManagement;

[DependsOn(
    typeof(SufiAbpTagsManagementApplicationContractsModule),
    typeof(AbpHttpClientModule)
)]
public class SufiAbpTagsManagementHttpApiClientModule : AbpModule
{
}

