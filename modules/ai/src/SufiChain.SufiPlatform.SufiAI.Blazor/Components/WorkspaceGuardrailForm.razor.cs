using System.Globalization;
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
            .Select(period => new WorkspaceGuardrailFormRow
            {
                Period = period,
                AmountText = current.FirstOrDefault(item => item.Period == period)?.AmountUsd
                    .ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty
            })
            .ToList();
    }

    public static bool TryBuildItems(IEnumerable<WorkspaceGuardrailFormRow> rows, out List<WorkspaceGuardrailDto> items)
    {
        items = new List<WorkspaceGuardrailDto>();
        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.AmountText))
            {
                continue;
            }

            if (!TryParseDecimal(row.AmountText, out var amount) || amount < 0)
            {
                items = [];
                return false;
            }

            if (amount == 0)
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

    private static bool TryParseDecimal(string value, out decimal result)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result) ||
               decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out result);
    }
}

public sealed class WorkspaceGuardrailFormRow
{
    public WorkspaceGuardrailPeriod Period { get; init; }

    public string AmountText { get; set; } = string.Empty;
}
