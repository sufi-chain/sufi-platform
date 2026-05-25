using SufiChain.SufiAbp.Http.Client;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.TagsManagement;

[DependsOn(
    typeof(SufiAbpTagsManagementApplicationContractsModule),
    typeof(SufiAbpHttpClientModule)
)]
public class SufiAbpTagsManagementHttpApiClientModule : AbpModule
{
}

