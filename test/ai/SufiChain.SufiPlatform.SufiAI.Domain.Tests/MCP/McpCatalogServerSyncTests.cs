using SufiChain.SufiPlatform.SufiAI.MCP.Cache;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.MCP;

public class McpCatalogServerSyncTests
{
    [Fact]
    public void TestConnection_Success_DiscoversThatServersTools()
    {
        var existing = new List<MCPToolDescriptor>
        {
            new() { Name = "external.other.keep" },
            new() { Name = "external.amar.old" }
        };
        var discovered = new List<MCPToolDescriptor>
        {
            new() { Name = "external.amar.one" },
            new() { Name = "external.amar.two" },
            new() { Name = "external.amar.three" }
        };

        var merged = McpCatalogServerSync.Apply(existing, "amar", connectionSucceeded: true, discovered);

        merged.Count.ShouldBe(4);
        merged.ShouldContain(tool => tool.Name == "external.other.keep");
        merged.ShouldNotContain(tool => tool.Name == "external.amar.old");
        merged.Count(tool => tool.Name.StartsWith("external.amar.")).ShouldBe(3);
    }

    [Fact]
    public void TestConnection_Failure_LeavesCatalogUnchanged()
    {
        var existing = new List<MCPToolDescriptor>
        {
            new() { Name = "external.other.keep" },
            new() { Name = "external.amar.old" }
        };

        var merged = McpCatalogServerSync.Apply(existing, "amar", connectionSucceeded: false, discovered: null);
        merged.Select(tool => tool.Name).ShouldBe(existing.Select(tool => tool.Name));
    }
}
