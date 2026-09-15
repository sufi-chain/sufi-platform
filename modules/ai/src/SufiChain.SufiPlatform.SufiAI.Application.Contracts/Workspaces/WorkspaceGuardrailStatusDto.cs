namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class WorkspaceGuardrailStatusDto
{
    public WorkspaceGuardrailPeriod Period { get; set; }

    public decimal LimitUsd { get; set; }

    public decimal UsedUsd { get; set; }

    public decimal RemainingUsd { get; set; }

    public bool IsExceeded { get; set; }

    public DateTime PeriodStartUtc { get; set; }
}
