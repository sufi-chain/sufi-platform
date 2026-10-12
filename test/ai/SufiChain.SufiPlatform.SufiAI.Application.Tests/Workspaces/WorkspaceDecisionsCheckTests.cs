using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.Workspaces;

public class WorkspaceDecisionsCheckTests
{
    [Fact]
    public void Same_id_is_kept()
    {
        WorkspaceDecisionsCheck
            .IsKept("respan/span-01-lite:free", "respan/span-01-lite:free")
            .ShouldBeTrue();
    }

    [Fact]
    public void Differing_case_and_padding_is_kept()
    {
        WorkspaceDecisionsCheck
            .IsKept("  Respan/Span-01-Lite:FREE  ", "respan/span-01-lite:free")
            .ShouldBeTrue();
    }

    [Fact]
    public void Nothing_sent_and_nothing_saved_is_kept()
    {
        WorkspaceDecisionsCheck.IsKept(null, null).ShouldBeTrue();
        WorkspaceDecisionsCheck.IsKept("   ", "  ").ShouldBeTrue();
    }

    [Fact]
    public void A_value_sent_but_not_saved_is_not_kept()
    {
        WorkspaceDecisionsCheck
            .IsKept("respan/span-01-lite:free", null)
            .ShouldBeFalse();
        WorkspaceDecisionsCheck
            .IsKept("respan/span-01-lite:free", "   ")
            .ShouldBeFalse();
    }

    [Fact]
    public void Nothing_sent_but_a_value_saved_is_not_kept()
    {
        WorkspaceDecisionsCheck
            .IsKept(null, "respan/span-01-lite:free")
            .ShouldBeFalse();
        WorkspaceDecisionsCheck
            .IsKept("   ", "respan/span-01-lite:free")
            .ShouldBeFalse();
    }

    [Fact]
    public void A_different_value_is_not_kept()
    {
        WorkspaceDecisionsCheck
            .IsKept("respan/span-01-lite:free", "typesafe/jev-1.13")
            .ShouldBeFalse();
    }
}
