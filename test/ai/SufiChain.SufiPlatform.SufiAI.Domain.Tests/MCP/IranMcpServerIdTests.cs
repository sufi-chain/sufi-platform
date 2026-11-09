using Shouldly;
using SufiChain.SufiPlatform.SufiAI.MCP;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.MCP;

public class IranMcpServerIdTests
{
    [Fact]
    public void Two_servers_in_two_environments_get_distinct_ids()
    {
        var stageTgju = IranMcpServerId.Derive("stage", "tgju-mcp");
        var stageDigikala = IranMcpServerId.Derive("stage", "digikala-mcp");
        var prodTgju = IranMcpServerId.Derive("Production", "tgju-mcp");
        var prodDigikala = IranMcpServerId.Derive("prod", "digikala-mcp");

        stageTgju.ShouldNotBe(stageDigikala);
        stageTgju.ShouldNotBe(prodTgju);
        stageDigikala.ShouldNotBe(prodDigikala);
        prodTgju.ShouldNotBe(prodDigikala);

        new HashSet<Guid> { stageTgju, stageDigikala, prodTgju, prodDigikala }.Count.ShouldBe(4);
    }

    [Fact]
    public void Derive_is_idempotent_and_case_insensitive()
    {
        var first = IranMcpServerId.Derive("Stage", "TGJU-MCP");
        var second = IranMcpServerId.Derive("stage", "tgju-mcp");
        first.ShouldBe(second);
        first.ShouldBe(Guid.Parse("b661a443-3470-5768-a596-7e22be7264b3"));
    }

    [Fact]
    public void Resolve_prefers_explicit_host_row_id()
    {
        var hostRowId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        IranMcpServerId.Resolve(hostRowId.ToString(), "stage", "tgju-mcp").ShouldBe(hostRowId);
        IranMcpServerId.Resolve(null, "stage", "tgju-mcp")
            .ShouldBe(IranMcpServerId.Derive("stage", "tgju-mcp"));
    }
}
