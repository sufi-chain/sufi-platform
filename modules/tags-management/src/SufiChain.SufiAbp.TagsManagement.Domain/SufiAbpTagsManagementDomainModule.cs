using Volo.Abp.Modularity;
using Volo.Abp.Domain;

namespace SufiChain.SufiAbp.TagsManagement;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(SufiAbpTagsManagementDomainSharedModule)
)]
public class SufiAbpTagsManagementDomainModule : AbpModule
{
}

