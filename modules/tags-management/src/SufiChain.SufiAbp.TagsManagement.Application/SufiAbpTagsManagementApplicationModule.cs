using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.TagsManagement;

[DependsOn(
    typeof(SufiAbpTagsManagementDomainModule),
    typeof(SufiAbpTagsManagementApplicationContractsModule),
    typeof(AbpMapperlyModule)
)]
public class SufiAbpTagsManagementApplicationModule : AbpModule
{
}

