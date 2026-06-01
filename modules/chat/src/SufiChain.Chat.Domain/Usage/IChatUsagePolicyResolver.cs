using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.Chat.Usage;

public interface IChatUsagePolicyResolver
{
    Task<ChatUsagePolicy> ResolveAsync(AccessMode accessMode, CancellationToken cancellationToken = default);

    Task<ChatAiUsagePolicy> ResolveAiAsync(CancellationToken cancellationToken = default);
}
