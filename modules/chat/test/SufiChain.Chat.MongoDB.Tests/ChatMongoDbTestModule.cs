using Volo.Abp.Modularity;

namespace SufiChain.Chat;

[DependsOn(
    typeof(ChatApplicationTestModule),
    typeof(ChatMongoDbModule)
)]
public class ChatMongoDbTestModule : AbpModule
{
}
