using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Tests;

public class UsageAnalyticsStatusFilterTests
{
    private static readonly bool[] Rows = [true, false, true, false];

    [Fact]
    public void UsageAnalytics_StatusFilterFailed_ShowsOnlyFailedRows()
    {
        UsageStatusFilter.Apply(Rows, "Failed", row => row).ShouldBe(new[] { false, false });
        UsageStatusFilter.Apply(Rows, "false", row => row).ShouldBe(new[] { false, false });
    }

    [Fact]
    public void UsageAnalytics_StatusFilterSuccess_ShowsOnlySuccessRows()
    {
        UsageStatusFilter.Apply(Rows, "Success", row => row).ShouldBe(new[] { true, true });
        UsageStatusFilter.Apply(Rows, "true", row => row).ShouldBe(new[] { true, true });
    }

    [Fact]
    public void UsageAnalytics_StatusFilterFailed_Fa_ShowsOnlyFailedRows()
    {
        UsageStatusFilter.Apply(Rows, "ناموفق", row => row).ShouldBe(new[] { false, false });
        UsageStatusFilter.Apply(Rows, "موفق", row => row).ShouldBe(new[] { true, true });
    }
}
