using System;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.Chat.Usage;

public interface IChatRateLimitCounterStore
{
    Task<long> IncrementAsync(string key, TimeSpan window, CancellationToken cancellationToken = default);
}
