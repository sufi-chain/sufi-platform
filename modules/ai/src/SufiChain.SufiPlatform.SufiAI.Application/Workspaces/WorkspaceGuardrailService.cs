using System;
using System.Collections.Generic;
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
        var statuses = await GetStatusAsync(workspaceId, cancellationToken);
        var exceeded = statuses.FirstOrDefault(x => x.IsExceeded);
        if (exceeded == null)
        {
            return;
        }

        throw new BusinessException(AIErrorCodes.WorkspaceGuardrailExceeded)
            .WithData("WorkspaceId", workspaceId)
            .WithData("Period", exceeded.Period.ToString())
            .WithData("LimitUsd", exceeded.LimitUsd)
            .WithData("UsedUsd", exceeded.UsedUsd);
    }

    public async Task<List<WorkspaceGuardrailStatusDto>> GetStatusAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        var workspace = await _workspaceRepository.GetAsync(workspaceId, includeDetails: true, cancellationToken);
        var now = DateTime.UtcNow;
        var statuses = new List<WorkspaceGuardrailStatusDto>();

        foreach (var guardrail in workspace.Guardrails.OrderBy(x => x.Period))
        {
            var start = GetPeriodStart(now, guardrail.Period);
            var consumed = await _usageLogRepository.GetTotalCostAsync(
                workspaceId,
                start,
                now,
                cancellationToken);

            statuses.Add(new WorkspaceGuardrailStatusDto
            {
                Period = guardrail.Period,
                LimitUsd = guardrail.AmountUsd,
                UsedUsd = consumed,
                RemainingUsd = Math.Max(0, guardrail.AmountUsd - consumed),
                IsExceeded = consumed >= guardrail.AmountUsd,
                PeriodStartUtc = start
            });
        }

        return statuses;
    }

    private static DateTime GetPeriodStart(DateTime now, WorkspaceGuardrailPeriod period)
    {
        return period switch
        {
            WorkspaceGuardrailPeriod.Daily => now.Date,
            WorkspaceGuardrailPeriod.Weekly => now.Date.AddDays(-(int)now.DayOfWeek),
            WorkspaceGuardrailPeriod.Monthly => new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc),
            _ => now.Date
        };
    }
}
