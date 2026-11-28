using SufiChain.SufiBlazor.Contracts.Data;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;

public static class UsageStatusFilter
{
    public static IReadOnlyList<SbFilterOption> Options(Func<string, string> localize) =>
    [
        new("true", localize("Success")),
        new("false", localize("Failed"))
    ];

    public static IReadOnlyList<T> Apply<T>(IEnumerable<T> rows, string? selected, Func<T, bool> isSuccess)
    {
        var want = Parse(selected);
        return want is null
            ? rows.ToList()
            : rows.Where(row => isSuccess(row) == want.Value).ToList();
    }

    public static bool? Parse(string? selected)
    {
        if (string.IsNullOrWhiteSpace(selected))
        {
            return null;
        }

        var text = selected.Trim();
        if (IsSuccess(text))
        {
            return true;
        }

        if (IsFailed(text))
        {
            return false;
        }

        return null;
    }

    private static bool IsSuccess(string text) =>
        text.Equals("true", StringComparison.OrdinalIgnoreCase)
        || text.Equals("Success", StringComparison.OrdinalIgnoreCase)
        || text.Equals("موفق", StringComparison.Ordinal);

    private static bool IsFailed(string text) =>
        text.Equals("false", StringComparison.OrdinalIgnoreCase)
        || text.Equals("Failed", StringComparison.OrdinalIgnoreCase)
        || text.Equals("ناموفق", StringComparison.Ordinal);
}
