using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.SufiCom.Chat;

public abstract class ChatApplicationTestBase<TStartupModule> : ChatTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
}
