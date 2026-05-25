using SufiChain.SufiAbp.Mapperly;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.TagsManagement;

[DependsOn(
    typeof(SufiAbpTagsManagementDomainModule),
    typeof(SufiAbpTagsManagementApplicationContractsModule),
    typeof(SufiAbpMapperlyModule)
)]
public class SufiAbpTagsManagementApplicationModule : AbpModule
{
}

