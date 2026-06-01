using Volo.Abp.Modularity;

namespace SufiChain.Chat;

public abstract class ChatDomainTestBase<TStartupModule> : ChatTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
}
