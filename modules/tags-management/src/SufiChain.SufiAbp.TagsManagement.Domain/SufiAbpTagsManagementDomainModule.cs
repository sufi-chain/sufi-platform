using SufiChain.SufiAbp.Ddd;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.TagsManagement;

[DependsOn(
    typeof(SufiAbpDddDomainModule),
    typeof(SufiAbpTagsManagementDomainSharedModule)
)]
public class SufiAbpTagsManagementDomainModule : AbpModule
{
}

