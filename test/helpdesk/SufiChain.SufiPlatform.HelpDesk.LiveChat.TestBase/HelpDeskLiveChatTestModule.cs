using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;

namespace SufiChain.SufiPlatform.HelpDesk.LiveChat;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpTestBaseModule),
    typeof(HelpDeskLiveChatDomainModule))]
public class HelpDeskLiveChatTestModule : AbpModule
{
}
