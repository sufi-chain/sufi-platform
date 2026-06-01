using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SufiChain.Chat.Usage;

public class NullChatUsageWalletResolver : IChatUsageWalletResolver, ITransientDependency
{
    public virtual Task<ChatUsageWalletContext?> ResolveAsync(
        ChatAiOperationContext context,
        CancellationToken ct = default)
    {
        return Task.FromResult<ChatUsageWalletContext?>(null);
    }
}
