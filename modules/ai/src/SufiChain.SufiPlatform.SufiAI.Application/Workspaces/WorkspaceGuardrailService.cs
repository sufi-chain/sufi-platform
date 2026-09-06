using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public sealed class WorkspaceGuardrailService : IWorkspaceGuardrailService, ITransientDependency
{
    private readonly IRepository<Workspace, Guid> _workspaceRepository;
    private readonly IAIUsageLogRepository _usageLogRepository;

    public WorkspaceGuardrailService(
        IRepository<Workspace, Guid> workspaceRepository,
        IAIUsageLogRepository usageLogRepository)
    {
        _workspaceRepository = workspaceRepository;
        _usageLogRepository = usageLogRepository;
    }

    public async Task EnsureCanExecuteAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var workspace = await _workspaceRepository.GetAsync(workspaceId, includeDetails: true, cancellationToken);
        var now = DateTime.UtcNow;

        foreach (var guardrail in workspace.Guardrails)
        {
            var start = GetPeriodStart(now, guardrail.Period);
            var consumed = await _usageLogRepository.GetTotalCostAsync(
                workspaceId,
                start,
                now,
                cancellationToken);

            if (consumed >= guardrail.AmountUsd)
            {
                throw new BusinessException(AIErrorCodes.WorkspaceGuardrailExceeded)
                    .WithData("WorkspaceId", workspaceId)
                    .WithData("Period", guardrail.Period.ToString())
                    .WithData("LimitUsd", guardrail.AmountUsd)
                    .WithData("UsedUsd", consumed);
            }
        }
    }

    private static DateTime GetPeriodStart(DateTime now, WorkspaceGuardrailPeriod period)
    {
        return period switch
        {
            WorkspaceGuardrailPeriod.Daily => now.Date,
            WorkspaceGuardrailPeriod.Weekly => now.Date.AddDays(-(int)now.DayOfWeek),
            WorkspaceGuardrailPeriod.Monthly => new DateTime(now.Year, now.Month, 1),
            _ => now.Date
        };
    }
}
