using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Permissions;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.AI;

public class AIModelCatalogAccessTests
{
    [Theory]
    [InlineData(AIPermissions.AI.Chat)]
    [InlineData(AIPermissions.WorkspaceChat.Default)]
    [InlineData(AIPermissions.Workspaces.Default)]
    public void Workspace_surfaces_can_load_the_catalog(string permission)
    {
        AIModelCatalogAccess.AllowsWorkspaceRoutes([permission]).ShouldBeTrue();
    }

    [Fact]
    public void Unrelated_permission_cannot_load_the_workspace_catalog()
    {
        AIModelCatalogAccess.AllowsWorkspaceRoutes([AIPermissions.AI.ViewUsage]).ShouldBeFalse();
        AIModelCatalogAccess.AllowsWorkspaceRoutes([]).ShouldBeFalse();
    }

    [Fact]
    public void Denial_names_the_required_permissions_and_an_anonymous_principal()
    {
        var signedIn = AIModelCatalogAccess.DescribeDenial(isAuthenticated: true);
        signedIn.ShouldContain(AIPermissions.AI.Chat);
        signedIn.ShouldContain(AIPermissions.WorkspaceChat.Default);
        signedIn.ShouldContain(AIPermissions.Workspaces.Default);
        signedIn.ShouldNotContain("anonymous");

        var anonymous = AIModelCatalogAccess.DescribeDenial(isAuthenticated: false);
        anonymous.ShouldContain("anonymous");
        anonymous.ShouldContain(AIPermissions.WorkspaceChat.Default);
    }
}
