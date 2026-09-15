using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.SufiCom.Chat;

public abstract class ChatDomainTestBase<TStartupModule> : ChatTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
}
