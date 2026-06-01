using Volo.Abp.Modularity;

namespace SufiChain.Chat;

[DependsOn(
    typeof(ChatDomainSharedModule)
)]
public class ChatConnectorModule : AbpModule
{
}
