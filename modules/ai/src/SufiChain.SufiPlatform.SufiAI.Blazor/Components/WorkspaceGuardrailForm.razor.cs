using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.SufiAI.Workspaces;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Components;

public partial class WorkspaceGuardrailForm : AIComponentBase
{
    [Parameter]
    public List<WorkspaceGuardrailFormRow> Rows { get; set; } = new();

    [Parameter]
    public IReadOnlyList<WorkspaceGuardrailStatusDto> Status { get; set; } = Array.Empty<WorkspaceGuardrailStatusDto>();

    [Parameter]
    public bool Disabled { get; set; }

    public static List<WorkspaceGuardrailFormRow> CreateRows(IEnumerable<WorkspaceGuardrailDto>? existing = null)
    {
        var current = existing?.ToList() ?? [];
        return Enum.GetValues<WorkspaceGuardrailPeriod>()
            .Select(period =>
            {
                var amount = current.FirstOrDefault(item => item.Period == period)?.AmountUsd;
                return new WorkspaceGuardrailFormRow
                {
                    Period = period,
                    Amount = amount is > 0 ? amount : null
                };
            })
            .ToList();
    }

    public static bool TryBuildItems(IEnumerable<WorkspaceGuardrailFormRow> rows, out List<WorkspaceGuardrailDto> items)
    {
        items = new List<WorkspaceGuardrailDto>();
        foreach (var row in rows)
        {
            if (row.Amount is not decimal amount || amount <= 0)
            {
                continue;
            }

            items.Add(new WorkspaceGuardrailDto
            {
                Period = row.Period,
                AmountUsd = amount
            });
        }

        return true;
    }

    private static double GetUsagePercent(WorkspaceGuardrailStatusDto status)
    {
        if (status.LimitUsd <= 0)
        {
            return status.IsExceeded ? 100 : 0;
        }

        var percent = (double)(status.UsedUsd / status.LimitUsd) * 100;
        return Math.Min(100, Math.Max(0, percent));
    }
}

public sealed class WorkspaceGuardrailFormRow
{
    public WorkspaceGuardrailPeriod Period { get; init; }

    public decimal? Amount { get; set; }
}
