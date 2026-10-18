using System;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.Workspaces;

public class WorkspaceUseWebSearchTests
{
    [Fact]
    public void New_workspace_allows_web_search_by_default()
    {
        var workspace = new Workspace(Guid.NewGuid(), "ws", AIProviderType.OpenAI, "gpt", null);

        workspace.UseWebSearch.ShouldBeTrue();
        new CreateWorkspaceDto().UseWebSearch.ShouldBeTrue();
        new UpdateWorkspaceDto().UseWebSearch.ShouldBeTrue();
    }

    [Fact]
    public void Live_inherited_view_copies_source_web_search_setting()
    {
        var source = new Workspace(Guid.NewGuid(), "host", AIProviderType.OpenAI, "gpt", null);
        source.SetUseWebSearch(false);
        var projection = new Workspace(Guid.NewGuid(), "tenant", AIProviderType.OpenAI, "gpt", Guid.NewGuid());
        projection.MarkAsInherited(source.Id, Guid.NewGuid());

        var view = projection.CreateLiveInheritedView(source);

        view.UseWebSearch.ShouldBeFalse();
    }

    [Theory]
    [InlineData(null, false, "https://gw/v1", true)]
    [InlineData("", false, null, true)]
    [InlineData("https://gw/v1/", false, "https://GW/v1", true)]
    [InlineData("https://gw/v1", true, "https://gw/v1", false)]
    [InlineData("https://other/v1", false, "https://gw/v1", false)]
    [InlineData("https://gw/v1", false, null, false)]
    public void Route_connection_mode_detects_workspace_endpoint(string? route, bool hasKey, string? workspace, bool expected)
    {
        WorkspaceConnectionMatcher.UsesWorkspaceConnection(route, hasKey, workspace).ShouldBe(expected);
    }
}
