using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Application.Tests.Hooshvare;

public class HooshvareWebSearchGateTests
{
    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, false)]
    public void Effective_web_search_requires_hooshvare_and_workspace(bool hooshvare, bool workspace, bool expected)
    {
        HooshvareWebSearchGate.Apply(hooshvare, new HooshvareWorkspaceBinding { UseWebSearch = workspace }).ShouldBe(expected);
    }

    [Fact]
    public void Missing_binding_keeps_hooshvare_setting()
    {
        HooshvareWebSearchGate.Apply(true, null).ShouldBeTrue();
    }
}
