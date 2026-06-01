using Volo.Abp.Modularity;

namespace SufiChain.Chat;

[DependsOn(typeof(ChatTestBaseModule))]
public class ChatDomainTestModule : AbpModule
{
}
