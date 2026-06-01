using Volo.Abp.Modularity;

namespace SufiChain.Chat;

public abstract class ChatApplicationTestBase<TStartupModule> : ChatTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
}
