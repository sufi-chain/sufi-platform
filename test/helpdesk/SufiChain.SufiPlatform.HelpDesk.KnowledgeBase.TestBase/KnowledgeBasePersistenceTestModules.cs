using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpTestBaseModule),
    typeof(HelpDeskKnowledgeBaseDomainModule))]
public class HelpDeskKnowledgeBaseTestModule : AbpModule
{
}
