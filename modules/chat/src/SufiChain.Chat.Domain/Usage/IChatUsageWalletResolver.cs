using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.Chat.Usage;

public interface IChatUsageWalletResolver
{
    Task<ChatUsageWalletContext?> ResolveAsync(ChatAiOperationContext context, CancellationToken ct = default);
}
