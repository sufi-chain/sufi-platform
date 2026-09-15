using SufiChain.SufiPlatform.SufiCom.Chat.Usage;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Supports;

public class TestChatUsageWalletResolver : IChatUsageWalletResolver, ISingletonDependency
{
    public ChatUsageWalletContext? Context { get; set; }

    public Task<ChatUsageWalletContext?> ResolveAsync(
        ChatAiOperationContext context,
        CancellationToken ct = default)
    {
        return Task.FromResult(Context);
    }
}
