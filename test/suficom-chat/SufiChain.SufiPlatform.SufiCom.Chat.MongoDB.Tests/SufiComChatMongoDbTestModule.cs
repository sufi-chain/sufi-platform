using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.SufiCom.Chat;

[DependsOn(
    typeof(MongoDB.SufiComChatMongoDbTestModule)
)]
public class SufiComChatMongoDbTestModule : AbpModule
{
}
