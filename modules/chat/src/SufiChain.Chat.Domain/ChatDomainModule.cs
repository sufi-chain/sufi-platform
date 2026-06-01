using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace SufiChain.Chat;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(ChatDomainSharedModule)
)]
public class ChatDomainModule : AbpModule
{
}
