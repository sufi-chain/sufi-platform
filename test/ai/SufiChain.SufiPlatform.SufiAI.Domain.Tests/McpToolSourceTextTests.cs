using SufiChain.SufiPlatform.SufiAI.MCP;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests;

public class McpToolSourceTextTests
{
    [Fact]
    public void McpTools_SourceFilter_ServerKeyAmar_MatchesAmarTools()
    {
        var text = McpToolSourceText.Format("Statistical", "external.amar.list");
        text.Contains("amar", StringComparison.OrdinalIgnoreCase).ShouldBeTrue();
    }

    [Fact]
    public void McpTools_SourceFilter_DisplayNameStatistical_StillMatches()
    {
        var text = McpToolSourceText.Format("Statistical", "external.amar.list");
        text.Contains("Statistical", StringComparison.OrdinalIgnoreCase).ShouldBeTrue();
        McpToolSourceText.Format("Identity", "identity.users").ShouldBe("Identity");
    }
}
