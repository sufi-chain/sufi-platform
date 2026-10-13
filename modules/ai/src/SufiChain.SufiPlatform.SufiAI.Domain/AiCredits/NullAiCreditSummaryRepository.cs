using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.AiCredits;

public class NullAiCreditSummaryRepository : IAiCreditSummaryRepository, ITransientDependency
{
    public Task<AICreditSummary?> GetSummaryAsync(
        Guid tenantId,
        DateTime? periodStart = null,
        DateTime? periodEnd = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult<AICreditSummary?>(null);

    public Task<List<AICreditSummary>> GetAllSummaryAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(new List<AICreditSummary>());
}
