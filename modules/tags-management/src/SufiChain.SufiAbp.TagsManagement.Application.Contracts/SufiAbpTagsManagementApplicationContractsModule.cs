using SufiChain.SufiAbp.Authorization;
using SufiChain.SufiAbp.Ddd;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.TagsManagement;

[DependsOn(
    typeof(SufiAbpTagsManagementDomainSharedModule),
    typeof(SufiAbpDddApplicationContractsModule),
    typeof(SufiAbpAuthorizationModule)
)]
public class SufiAbpTagsManagementApplicationContractsModule : AbpModule
{
}

