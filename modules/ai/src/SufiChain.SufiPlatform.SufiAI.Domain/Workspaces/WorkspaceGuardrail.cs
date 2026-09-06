using System;
using Volo.Abp.Domain.Entities;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class WorkspaceGuardrail : Entity<Guid>
{
    public Guid WorkspaceId { get; protected set; }
    public WorkspaceGuardrailPeriod Period { get; protected set; }
    public decimal AmountUsd { get; protected set; }

    protected WorkspaceGuardrail() { }

    public WorkspaceGuardrail(Guid id, Guid workspaceId, WorkspaceGuardrailPeriod period, decimal amountUsd)
        : base(id)
    {
        WorkspaceId = workspaceId;
        Set(period, amountUsd);
    }

    public void Set(WorkspaceGuardrailPeriod period, decimal amountUsd)
    {
        if (amountUsd < 0) throw new Volo.Abp.BusinessException("AI:InvalidGuardrailAmount");
        Period = period;
        AmountUsd = amountUsd;
    }
}
