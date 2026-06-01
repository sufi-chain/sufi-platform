using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.Chat.Usage;

public interface IChatUsageCounterRepository : IRepository<ChatUsageCounter, Guid>
{
    Task<long> GetCountAsync(
        Guid? tenantId,
        string counterKey,
        ChatUsageCounterPeriod period,
        DateTime periodStart,
        CancellationToken cancellationToken = default);

    Task IncrementAsync(
        Guid? tenantId,
        string counterKey,
        ChatUsageCounterPeriod period,
        DateTime periodStart,
        DateTime periodEnd,
        long count = 1,
        long tokenCount = 0,
        CancellationToken cancellationToken = default);
}
